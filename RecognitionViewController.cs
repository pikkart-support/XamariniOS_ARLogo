﻿using System;
using Foundation;
using UIKit;
using Pikkart.ArSdk.Recognition;
using System.Runtime.InteropServices;
using GLKit;
using CoreGraphics;
using OpenGLES;
using OpenTK.Graphics.ES20;

namespace XamariniOS_ARLogo
{
	
	public class RecognitionViewController : PKTRecognitionController,IPKTIRecognitionListener
	{
		nfloat ViewportWidth, ViewportHeight;
		int Angle;
        bool firstGLUpdate=false;
        Mesh mMonkeyMesh;
        Mesh mMonkeyMeshYellow;
        Mesh mMonkeyMeshBlue;
        Mesh mMonkeyMeshRed;
        Mesh mMonkeyMeshGray;

		#region IPKTIRecognitionListener methods
		[Export ("executingCloudSearch")]
		void ExecutingCloudSearch() {
			Console.WriteLine("ExecutingCloudSearch called!");
		}
		[Export ("cloudMarkerNotFound")]
		void CloudMarkerNotFound() {
			Console.WriteLine("CloudMarkerNotFound called!");
		}
		[Export ("internetConnectionNeeded")]
		void InternetConnectionNeeded() {
			Console.WriteLine("InternetConnectionNeeded called!");
		}
		[Export ("markerFound:")]
		void MarkerFound(PKTMarker marker) {
			Console.WriteLine("MarkerFound called with id = {0}!",marker.Id);
			/*
			float[] projArray = new float[16];
			float[] modelArray = new float[16];
			IntPtr projMatrixPtr = IntPtr.Zero, modelMatrixPtr = IntPtr.Zero;
			GetCurrentProjectionMatrix(ref projMatrixPtr);
			GetCurrentModelViewMatrix(ref modelMatrixPtr);

 			Marshal.Copy(projMatrixPtr, projArray, 0, 16);
			Marshal.Copy(modelMatrixPtr, modelArray,0,16);	
			*/
		
			//Console.WriteLine("projMatrix[0] = {0}, projMatrix[1] = {1}", projArray[0], projArray[1]);
			//Console.WriteLine("modelMatrix[0] = {0}, modelMatrix[1] = {1}", modelArray[0], modelArray[1]); 
		}
		[Export ("markerNotFound")]
		void markerNotFound() {
			Console.WriteLine("markerNotFound called!");
		}

		[Export ("markerTrackingLost:")]
		void MarkerTrackingLost(string markerId) {
			Console.WriteLine("MarkerTrackingLost called! with Id = {0}", markerId);
		}
		
        [Export ("ARLogoFound:withCode:")]
        void ARLogoFound(string markerId, NSNumber code) {
            Console.WriteLine("ARLogoFound called! with Id = {0} and patterCode =  {1}", markerId, code.Int64Value);

            switch (code.Int64Value) {
                case 97703:
                    SelectMonkey(monkeyID: 1);
                break;
                case 84895:
                    SelectMonkey(monkeyID: 2);
                break;
                case 65266:
                    SelectMonkey(monkeyID: 3);
                break;
                default:
                   SelectMonkey(monkeyID: 1);
                break;
            }
        }

        [Export ("markerEngineToUpdate:")]
        void MarkerEngineToUpdate(string markerId) {
            Console.WriteLine("MarkerEngineToUpdate called! with Id = {0}", markerId);

        }

		#endregion
		void ApplyCameraGlOrientation(UIInterfaceOrientation orientation) {
			UIScreen mainScreen = UIScreen.MainScreen;
			CGRect boundScreen = mainScreen.Bounds;
			nfloat scale = mainScreen.Scale;
			nfloat widthPort = boundScreen.Size.Width*scale;
			nfloat heightPort = boundScreen.Size.Height*scale;
			
			if (scale == 3) // iPhone 6 plus is not a  @3x, a downsampling applies...
                    		// http://www.paintcodeapp.com/news/iphone-6-screens-demystified
    		{
        		widthPort=widthPort/1.15f;
        		heightPort=heightPort/1.15f;
    		}
    		
    		Angle=90;
            ViewportWidth=widthPort;
    		ViewportHeight=heightPort;

    		switch (orientation) {
        	case UIInterfaceOrientation.Portrait:
           		Angle=90;
     		break;
        	case UIInterfaceOrientation.LandscapeRight:
            	Angle=0;
            	break;
        	case UIInterfaceOrientation.LandscapeLeft:
            	Angle=180;
            	break;
            case UIInterfaceOrientation.PortraitUpsideDown:
				Angle=270;
            	break;
        	default:
            	break;
			}
		}
		
		private RecognitionViewController (IntPtr handle) : base (handle)
		{
			// Note: this .ctor should not contain any initialization logic.
		}
		
		public RecognitionViewController ()
		{
			// Note: this .ctor should not contain any initialization logic.
		}
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			// Perform any additional setup after loading the view, typically from a nib.
			if (View is GLKView) {
				EAGLContext.SetCurrentContext(((GLKView)View).Context);
                /*
                _monkeyMeshYellow = Mesh(meshFile: Bundle.main.path(forResource: "monkey", ofType: "json")!,
                           textureFile: Bundle.main.path(forResource: "texture", ofType: "png")!)
        _monkeyMeshBlue = Mesh(meshFile: Bundle.main.path(forResource: "monkey", ofType: "json")!,
                           textureFile: Bundle.main.path(forResource: "texture2", ofType: "png")!)
        _monkeyMeshRed = Mesh(meshFile: Bundle.main.path(forResource: "monkey", ofType: "json")!,
                               textureFile: Bundle.main.path(forResource: "texture3", ofType: "png")!)
        _monkeyMeshGray = Mesh(meshFile: Bundle.main.path(forResource: "monkey", ofType: "json")!,
                              textureFile: Bundle.main.path(forResource: "texturegray", ofType: "png")!)
                */
                string meshFile=NSBundle.MainBundle.PathForResource("monkey", "json");
                string textureFile=NSBundle.MainBundle.PathForResource("texture", "png");
                string textureFile2=NSBundle.MainBundle.PathForResource("texture2", "png");
                string textureFile3=NSBundle.MainBundle.PathForResource("texture3", "png");
                string textureFileGray=NSBundle.MainBundle.PathForResource("texturegray", "png");

                mMonkeyMeshYellow = new Mesh();
                mMonkeyMeshYellow.InitMesh(meshFile,textureFile);
                mMonkeyMeshYellow.LoadMesh();
                
                mMonkeyMeshBlue = new Mesh();
                mMonkeyMeshBlue.InitMesh(meshFile,textureFile2);
                mMonkeyMeshBlue.LoadMesh();

                mMonkeyMeshRed = new Mesh();
                mMonkeyMeshRed.InitMesh(meshFile,textureFile3);
                mMonkeyMeshRed.LoadMesh();

                mMonkeyMeshGray = new Mesh();
                mMonkeyMeshGray.InitMesh(meshFile,textureFileGray);
                mMonkeyMeshGray.LoadMesh();    
        
                mMonkeyMesh=mMonkeyMeshGray;

                GL.ClearColor(1, 1, 1, 1);
			}
			
			string[] dbNames={""};
			PKTCloudRecognitionInfo info = new PKTCloudRecognitionInfo(dbNames);
			PKTRecognitionOptions options = new PKTRecognitionOptions(PKTRecognitionStorage.PKTLOCAL, 
																      PKTRecognitionMode.PKTRECOGNITION_CONTINUOS_SCAN,
																	  info);
			ApplyCameraGlOrientation(UIApplication.SharedApplication.StatusBarOrientation);
			StartRecognition(options,this);
		}

        public void SelectMonkey(int monkeyID) {

            switch (monkeyID) {
                case 0:
                    mMonkeyMesh=mMonkeyMeshGray;
                break;
                case 1:
                    mMonkeyMesh=mMonkeyMeshYellow;
                break;
                case 2:
                    mMonkeyMesh=mMonkeyMeshBlue;
                break;
                case 3:
                    mMonkeyMesh=mMonkeyMeshRed;
                break;
                default:
                    mMonkeyMesh=mMonkeyMeshGray;
                break;
            }
        }

		public override void DidReceiveMemoryWarning ()
		{
			base.DidReceiveMemoryWarning ();
			// Release any cached data, images, etc that aren't in use.
		}
		
		bool computeModelViewProjectionMatrix(ref float[] mvpMatrix)  {
            float w = 640, h = 480;
            
            RenderUtils.MtxLoadIdentity(ref mvpMatrix);
            
            float ar = (float)(ViewportHeight/ViewportWidth);
            
            if (ViewportHeight > ViewportWidth)  {ar = 1f / ar;}
            
            float h1 = h, w1 = w;
            
            if (ar < h/w) {
                h1 = (float)(w * ar);
            }
            else {
                w1 = (float)(h / ar);
            }
            
            float a = 0, b = 0;
            
            switch (Angle) {
            case 0:
                a = 1; b = 0;
           	break;
            case 90:
                a = 0; b = 1;
            break;
            case 180:
                a = -1; b = 0;
                break;
            case 270:
                a = 0; b = -1;
                break;
            default: break;
            }
            
            float [] angleMatrix = {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0};
            
            angleMatrix[0] = a; angleMatrix[1] = b; angleMatrix[2]=0; angleMatrix[3] = 0;
            angleMatrix[4] = -b; angleMatrix[5] = a; angleMatrix[6] = 0; angleMatrix[7] = 0;
            angleMatrix[8] = 0; angleMatrix[9] = 0; angleMatrix[10] = 1f; angleMatrix[11] = 0f;
            angleMatrix[12] = 0f; angleMatrix[13] = 0f; angleMatrix[14] = 0f; angleMatrix[15] = 1f;
            
            IntPtr tempPtr=IntPtr.Zero;
            
            GetCurrentProjectionMatrix(ref tempPtr);
            
            float[] projectionMatrix = new float[16];
            
            Marshal.Copy(tempPtr,projectionMatrix, 0,16);
            
            projectionMatrix[5] = projectionMatrix[5] * (h / h1);
            
            float[] correctedProjection = {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0};
            
            RenderUtils.MatrixMultiply(4,  4,  ref angleMatrix,  4,  4, ref projectionMatrix, ref correctedProjection);
            
            if (isTracking()) {
            	IntPtr modelviewMatrixPtr=IntPtr.Zero;
                float[] temp_mvp = {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0};
                float[] modelviewMatrix=new float[16];
                
                GetCurrentModelViewMatrix(ref modelviewMatrixPtr);
                
                Marshal.Copy(modelviewMatrixPtr, modelviewMatrix,0,16);
    
                RenderUtils.MatrixMultiply(4,  4, ref correctedProjection,  4,  4, ref modelviewMatrix, ref temp_mvp);
                RenderUtils.MtxTranspose(ref temp_mvp, ref mvpMatrix);
                
                return true;
            }
            
            return false;
            
        }
    
    	
		public override void DrawInRect (GLKView view, CoreGraphics.CGRect rect)
    	{
    		if (!isActive()) return;

            UIInterfaceOrientation orientation = UIApplication.SharedApplication.StatusBarOrientation;
            CGSize size=new CGSize(ViewportWidth, ViewportHeight);
    		GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if (firstGLUpdate == false) {
                ApplyCameraGlOrientation(orientation);
                firstGLUpdate=true;
            }

    		RenderCamera(size,Angle);
    		
    		if (isTracking()) {
    			
            	if (CurrentMarker != null) {
                	if (CurrentMarker.Id == "3_1836") {
                		float[] mvpMatrix = {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0};
                    	if (computeModelViewProjectionMatrix(ref mvpMatrix)) {
                        	mMonkeyMesh.DrawMesh(ref mvpMatrix);
                        	RenderUtils.CheckGLError();
                    	}
					}
				}
			}
		    GL.Finish();
		}
    	
		public override void ViewWillTransitionToSize (CGSize toSize, IUIViewControllerTransitionCoordinator coordinator)
		{
			base.ViewWillTransitionToSize (toSize, coordinator);
			coordinator.AnimateAlongsideTransition((obj) => {
														UIInterfaceOrientation orientation = UIApplication.SharedApplication.StatusBarOrientation;
												    	ApplyCameraGlOrientation(orientation);
													},null);
		}
	}
}
