using System;
using System.Collections.Generic;

namespace Praeclarum.Graphics
{
	public class Stroke
	{
		PointF[] _apoints = null;
		readonly List<PointF> _points = new List<PointF> ();
		
		public PointF[] Points { 
			get { 
				if (_apoints == null) {
					_apoints = _points.ToArray ();
				}
				return _apoints;
			}
		}
		
		public DateTime CreatedTime { get; private set; }
		public TimeSpan Duration { get; set; }

		public Stroke ()
			: this (DateTime.UtcNow)
		{
		}

		public Stroke (DateTime createdTime)
		{
			CreatedTime = createdTime;
		}
		
		public void AddPoint (PointF rawPoint)
		{
			_points.Add (rawPoint);
			_apoints = null;
			_bbvalid = false;
		}

		public void AddPoints (IEnumerable<PointF> rawPoints)
		{
			_points.AddRange (rawPoints);
			_apoints = null;
			_bbvalid = false;
		}
		
		bool _bbvalid = false;
		RectangleF _bb;
		public RectangleF BoundingBox {
			get {
				if (!_bbvalid) {
					if (_points.Count > 0) {
						_bb = GetSegment (0).BoundingBox;
					}
					else {
						_bb = new RectangleF (0, 0, 0, 0);
					}
					_bbvalid = true;
				}
				return _bb;
			}
		}
		
		public StrokeSegment GetSegment (int startIndex)
		{
			return GetSegment (startIndex, _points.Count - 1);
		}
		
		public StrokeSegment GetSegment (int startIndex, int lastIndex)
		{
			return new StrokeSegment (this, startIndex, lastIndex);
		}

		public const float DefaultThickness = 4;
		
		public virtual void Draw (IGraphics g, float thickness = DefaultThickness)
		{
			Draw (g, 0, _points.Count, thickness);
		}
		
		protected virtual void Draw (IGraphics g, int startIndex, int length, float thickness)
		{
			g.BeginEntity (this);
			
			if (length == 0) {
				
			}
			else if (length == 1) {
				var p = _points [startIndex];
				var r = thickness / 2;
				g.FillOval (p.X - r, p.Y - r, thickness, thickness);
			}
			else {			
				g.BeginLines (true);
				
				var end = startIndex + length;
				for (var i = startIndex; i < end - 1; i++) {
					g.DrawLine (
						_points [i].X,
						_points [i].Y,
						_points [i + 1].X,
						_points [i + 1].Y,
						thickness);
				}
				
				g.EndLines ();
			}
		}
		
		#region Recognition
		
		public bool IsHorizontalLine {
			get {
				int e;
				return RecognizeHorizontalLine (0, out e);
			}
		}
		
		public bool RecognizeHorizontalLine (int startIndex, out int endIndex)
		{
			endIndex = startIndex;
			
			var segs = GetDirectionSegments (startIndex);
			
			if (segs.Length > 0 && segs [0].Direction.IsHorizontal ()) {				
				endIndex = segs [0].EndIndex;
				return true;
			}
			
			return false;
		}
		
		public bool IsVerticalLine {
			get {
				int e;
				return RecognizeVerticalLine (0, out e);
			}
		}
		
		public bool RecognizeVerticalLine (int startIndex, out int lastIndex)
		{
			lastIndex = startIndex;
			
			var segs = GetDirectionSegments (startIndex);
			
			if (segs.Length > 0 && segs [0].Direction.IsVertical ()) {				
				lastIndex = segs [0].EndIndex;
				return true;
			}
			
			return false;
		}

		public bool IsLoop {
			get {
				int e;
				return RecognizeLoop (0, out e);
			}
		}
		
		public bool RecognizeLoop (int startIndex, out int lastIndex)
		{
			lastIndex = startIndex;
			
			if (startIndex >= _points.Count) return false;
			
			var bb = BoundingBox;
			
			var maxDim = Math.Max (bb.Width, bb.Height);
			
			var segs = GetDirectionSegments (startIndex);
			
			if (segs.Length >= 4) {
				
				var fdir = segs [0].Direction;
				var sdir = segs [1].Direction;
				
				var ddir = sdir.Minus (fdir);
				
				if (Math.Abs (ddir) == 1) {
					
					//
					// Complete the loop
					//
					var lastSeg = 2;
					for (var i = 2; i < 4; i++) {
						
						if (segs [i].Direction.Minus (sdir) == ddir) {
							sdir = segs [i].Direction;
						}
						else {
							//
							// Is it "close enough"
							//
							var d = segs [i].StartPoint.DistanceTo (segs [0].StartPoint);
							if (d < maxDim * 0.25f) {
								break;
							}
							else {
								return false;
							}
						}
						
						lastSeg = i;
					}
					
					//
					// Allow the loop to fold in on itself
					//
					var ei = segs [lastSeg].EndIndex;
					if (lastSeg + 1 < segs.Length && segs [lastSeg + 1].Direction == fdir) {
						ei = segs [lastSeg + 1].EndIndex;
					}
					
					//
					// Trim the tail of the loop to the beginning of the loop
					//
					lastIndex = GetSegment (ei / 2, ei).GetClosestPoint (Points [0]);
					
					return true;
				}
			}

			return false;
		}
		
		public bool IntersectsWith (Stroke other)
		{
			return GetIntersectionWith (other) != null;
		}

		public bool IntersectsWith (LineSegmentF lineSegment)
		{
			var other = new Stroke ();
			other.AddPoint (lineSegment.Start);
			other.AddPoint (lineSegment.End);
			return GetIntersectionWith (other) != null;
		}
		
		public class IntersectionInfo
		{
			public PointF Location;
			
			public IntersectionInfo (PointF loc)
			{
				Location = loc;
			}
		}
		
		public IntersectionInfo GetIntersectionWith (Stroke other)
		{
			for (var i = 0; i < _points.Count - 1; i++) {
				
				var x1 = _points [i].X;
				var y1 = _points [i].Y;
				var x21 = _points [i + 1].X - x1;
				var y21 = _points [i + 1].Y - y1;
				
				for (var j = 0; j < other._points.Count - 1; j++) {
					
					var x3 = other._points [j].X;
					var y3 = other._points [j].Y;
					var x43 = other._points [j + 1].X - x3;
					var y43 = other._points [j + 1].Y - y3;
					var x13 = x1 - x3;
					var y13 = y1 - y3;
					
					var d = y43*x21 - x43*y21;
					if (d == 0.0f) continue;
					
					var na = x43*y13 - y43*x13;
					var ua = na / d;
					
					if (ua < 0 || ua > 1) continue;
					
					var nb = x21*y13 - y21*x13;
					var ub = nb / d;
					
					if (ub >= 0 && ub <= 1) {
						var loc = new PointF (x1 + ua * x21, y1 + ua * y21);
						return new IntersectionInfo (loc);
					}
				}
			}
			
			return null;
		}
		
		#endregion

		#region Simplification

		int RemoveInitialFlourish ()
		{
			var stroke = this;

			var points = stroke.Points;

			var totalLength = 0.0f;
			for (var i = 0; i < points.Length - 1; i++) {
				totalLength += points [i].DistanceTo (points [i+1]);
			}

			//
			// Look for a sharp bend within the first bit of the shape
			//
			var len = 0.0f;
			var lastDir = new PointF ();
			for (var i = 0; i < points.Length - 1; i++) {

				var dir = points [i+1].Subtract (points [i]).Normalized ();

				if (i > 0) {
					var dot = lastDir.Dot (dir);
					if (dot < 0) {
						//
						// Big turn
						//
						//stroke.WriteSvg ("/Users/fak/Desktop/turn.svg");
						return i;
					}
				}

				lastDir = dir;

				//
				// Only look at the first 5%
				//
				len += points [i].DistanceTo (points [i+1]);

				if (len > totalLength * 0.05f) {
					return 0;
				}
			}

			return 0;
		}

		public StrokeSegment[] GetResolutionSimplifiedSegments (int startIndex, int resolution)
		{
			var bb = BoundingBox;
			return GetSimplifiedSegments (startIndex, Math.Max (bb.Width, bb.Height) / resolution, 1000);
		}

		public Stroke GetSimplifiedStroke (int startIndex, float error, int maxSegments)
		{
			var segs = GetSimplifiedSegments (startIndex, error, maxSegments);
			var ss = new Stroke (CreatedTime);

			ss.AddPoint (segs [0].StartPoint);
			foreach (var s in segs) {
				ss.AddPoint (s.EndPoint);
			}

			return ss;
		}

		public StrokeSegment[] GetSimplifiedSegments (int startIndex, float error, int maxSegments)
		{
			var bb = BoundingBox;
			var points = Points;

			if (startIndex == 0) {
				startIndex = RemoveInitialFlourish ();
			}

			var totalSegment = new StrokeSegment (this, startIndex, points.Length - 1);

			var segments = new List<StrokeSegment> { totalSegment };

			//
			// Subdivide
			//
			for (;;) {
				//
				// Find the next to split
				//
				int segToSplit = -1;
				int pointToSplit = -1;
				float pointDist = 0;

				for (var i = 0; i < segments.Count; i++) {

					float d;
					var j = segments [i].GetFarthestInteriorPoint (out d);
					if (j >= 0 && (segToSplit == -1 || d > pointDist)) {
						segToSplit = i;
						pointToSplit = j;
						pointDist = d;
					}

				}

				//
				// Split it
				//
				var shouldSplit = segToSplit >= 0 &&
				                  segments.Count < maxSegments &&
				                  pointDist > error;

				if (shouldSplit) {
					var lastIndex = segments [segToSplit].EndIndex;
					segments [segToSplit].EndIndex = pointToSplit;
					segments.Insert (segToSplit + 1, new StrokeSegment (this, pointToSplit, lastIndex));
				}
				else {
					break;
				}
			}

			return segments.ToArray ();
		}

		#endregion

		#region directions

		public StrokeSegment[] GetDirectionSegments (int startIndex, int resolution = 20)
		{
			var segments = GetResolutionSimplifiedSegments (startIndex, resolution);

			//
			// Merge like-directions
			//
			/*			using (var w = new System.IO.StreamWriter ("/Users/fak/Desktop/simplified.svg")) {
				var points = stroke.Points;
				var svg = new CrossGraphics.Svg.SvgGraphics (w, stroke.BoundingBox);
				svg.BeginDrawing ();
				foreach (var s in segments) {
					svg.DrawLine (points [s.StartIndex], points [s.LastIndex], 3);
				}
				svg.EndDrawing ();
			}*/

			var merged = new List<StrokeSegment> ();

			for (var i = 0; i < segments.Length;) {

				var dir = segments [i].Direction;
				var j = i + 1;
				for (; j < segments.Length && segments [j].Direction == dir; j++) {
				}

				segments [i].EndIndex = j-1 < segments.Length ? segments [j-1].EndIndex : segments [segments.Length-1].EndIndex;
				merged.Add (segments [i]);

				i = j;
			}

			/*			using (var w = new System.IO.StreamWriter ("/Users/fak/Desktop/am.svg")) {
				var svg = new CrossGraphics.Svg.SvgGraphics (w, bb);
				svg.BeginDrawing ();
				foreach (var s in merged) {
					svg.DrawLine (points [s.StartIndex], points [s.LastIndex], 3);
				}
				svg.EndDrawing ();
			}*/

			return merged.ToArray ();
		}

		#endregion
	}

	public enum StrokeDirection : int
	{
		Right = 0,
		Up = 1,
		Left = 2,
		Down = 3,
	}

	public static class StrokeDirectionEx
	{
		public static int Minus (this StrokeDirection a, StrokeDirection b)
		{
			var d = (int)a - (int)b;

			if (d == 3) {
				d = -1;
			}
			else if (d == -3) {
				d = 1;
			}

			return d;
		}

		public static bool IsVertical (this StrokeDirection d)
		{
			return (d == StrokeDirection.Up) || (d == StrokeDirection.Down);
		}

		public static bool IsHorizontal (this StrokeDirection d)
		{
			return (d == StrokeDirection.Left) || (d == StrokeDirection.Right);
		}

		public static StrokeDirection Flipped (this StrokeDirection d)
		{
			if (d == StrokeDirection.Right) return StrokeDirection.Left;
			else if (d == StrokeDirection.Left) return StrokeDirection.Right;
			else if (d == StrokeDirection.Up) return StrokeDirection.Down;
			else return StrokeDirection.Up;
		}
	}

	public class StrokeSegment
	{
		public int StartIndex { get; private set; }

		int lastIndex = 0;
		public int EndIndex {
			get { return lastIndex; }
			set {
				lastIndex = value;
				SetDirection ();
			}
		}

		public PointF StartPoint { get { return _points [StartIndex]; } }
		public PointF EndPoint { get { return _points [EndIndex]; } }

		readonly PointF[] _points;

		public StrokeDirection Direction { get; private set; }

		public StrokeSegment (Stroke stroke, int startIndex, int lastIndex)
		{
			_points = stroke.Points;

			if (startIndex >= _points.Length) throw new ArgumentOutOfRangeException ("startIndex");
			if (lastIndex >= _points.Length) throw new ArgumentOutOfRangeException ("lastIndex");

			StartIndex = startIndex;
			EndIndex = lastIndex;
		}

		public RectangleF BoundingBox
		{
			get {
				var d = Stroke.DefaultThickness;
				var r = d / 2;
				var p = _points [0];
				var b = new RectangleF (p.X - r, p.Y - r, d, d);
				for (var i = StartIndex; i <= EndIndex; i++) {
					p = _points [i];
					var pb = new RectangleF (p.X - r, p.Y - r, d, d);
					b = RectangleF.Union (b, pb);
				}
				return b;
			}
		}

		public int GetClosestPoint (PointF p/*, out float dist*/)
		{
			var minIndex = -1;
			var minDist = 0.0f;

			for (var i = StartIndex; i <= EndIndex; i++) {

				var d = _points [i].DistanceTo (p);

				if (minIndex == -1 || (d < minDist)) {
					minIndex = i;
					minDist = d;
				}				
			}

			//dist = minDist;
			return minIndex;
		}

		public int GetFarthestInteriorPoint (out float dist)
		{
			var maxIndex = -1;
			var maxDist = 0.0f;

			var p1 = _points [StartIndex];
			var p2 = _points [EndIndex];

			for (var i = StartIndex + 1; i < EndIndex; i++) {

				var d = _points [i].DistanceToLine (p1, p2);

				if (maxIndex == -1 || (d > maxDist)) {
					maxIndex = i;
					maxDist = d;
				}				
			}

			dist = maxDist;
			return maxIndex;
		}





		public void WriteSvg (string path)
		{
			using (var w = new System.IO.StreamWriter (path)) {
				var svg = new Praeclarum.Graphics.SvgGraphics (w, BoundingBox);
				svg.BeginDrawing ();
				for (var i = StartIndex; i < EndIndex; i++) {
					svg.DrawLine (_points [i], _points [i + 1], Stroke.DefaultThickness);
				}
				svg.EndDrawing ();
			}
		}

		void SetDirection ()
		{
			var dx = _points [EndIndex].X - _points [StartIndex].X;
			var dy = _points [EndIndex].Y - _points [StartIndex].Y;

			if (Math.Abs (dx) >= Math.Abs (dy)) {
				Direction = dx >= 0 ? StrokeDirection.Right : StrokeDirection.Left;
			}
			else {
				Direction = dy >= 0 ? StrokeDirection.Down : StrokeDirection.Up;
			}
		}

		public override string ToString ()
		{
			return string.Format ("[Index={0}, LastIndex={1}, Direction={2}]", StartIndex, EndIndex, Direction);
		}
	}

}

