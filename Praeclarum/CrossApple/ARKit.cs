#if !__IOS__

using Foundation;

namespace ARKit {
    public class ARAnchor : NSObject
    {
    }
    public class ARCoachingOverlayView : UIKit.UIView
    {
    }
    public class ARSCNView : SceneKit.SCNView
    {
        public ARSCNView () : base (new CoreGraphics.CGRect(0, 0, 640, 480)) { }
    }
    public class ARSCNViewDelegate : NSObject
    {
        public virtual void DidAddNode (SceneKit.ISCNSceneRenderer renderer, SceneKit.SCNNode node, ARAnchor anchor) {}
    }    
}

#endif
