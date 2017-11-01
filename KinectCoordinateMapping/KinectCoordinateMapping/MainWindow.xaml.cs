using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KinectCoordinateMapping
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CameraMode _mode = CameraMode.Color;

        KinectSensor _sensor;
        Skeleton[] _bodies = new Skeleton[6];
        int x = 0;
        int y;
        double lastAngleZ;
        double lastAngleX;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _sensor = KinectSensor.KinectSensors.Where(s => s.Status == KinectStatus.Connected).FirstOrDefault();

            if (_sensor != null)
            {
                _sensor.ColorStream.Enable();
                _sensor.DepthStream.Enable();
                _sensor.SkeletonStream.Enable();

                _sensor.AllFramesReady += Sensor_AllFramesReady;

                _sensor.Start();
            }
        }

        void Sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            // Color
            using (var frame = e.OpenColorImageFrame())
            {
                if (frame != null)
                {
                    if (_mode == CameraMode.Color)
                    {
                        camera.Source = frame.ToBitmap();
                    }
                }
            }

            // Depth
            using (var frame = e.OpenDepthImageFrame())
            {
                if (frame != null)
                {
                    if (_mode == CameraMode.Depth)
                    {
                        camera.Source = frame.ToBitmap();
                    }
                }
            }

            // Body
            using (var frame = e.OpenSkeletonFrame())
            {
                if (frame != null)
                {
                    canvas.Children.Clear();

                    frame.CopySkeletonDataTo(_bodies);

                    foreach (var body in _bodies)
                    {
                        if (body.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            // COORDINATE MAPPING
                            foreach (Joint joint in body.Joints)
                            {
                                // 3D coordinates in meters
                                SkeletonPoint skeletonPoint = joint.Position;

                                // 2D coordinates in pixels
                                Point point = new Point();


                                x += 1;

                                if (x >= 100)
                                {
                                    var ArmRightX = body.Joints[JointType.HandRight].Position.X;    var ArmRightY = body.Joints[JointType.HandRight].Position.Y;    var ArmRightZ = body.Joints[JointType.HandRight].Position.Z;

                                    var ArmLeftX = body.Joints[JointType.HandLeft].Position.X; var ArmLeftY = body.Joints[JointType.HandLeft].Position.Y; var ArmLeftZ = body.Joints[JointType.HandLeft].Position.Z;

                                    var ShoulderRightX = body.Joints[JointType.ShoulderRight].Position.X;   var ShoulderRightY = body.Joints[JointType.ShoulderRight].Position.Y;   var ShoulderRightZ = body.Joints[JointType.ShoulderRight].Position.Z;

                                    var ShoulderLeftX = body.Joints[JointType.ShoulderLeft].Position.X; var ShoulderLeftY = ShoulderRightY; var ShoulderLeftZ = ShoulderRightZ;

                                    var HipRightX = ShoulderRightX; var HipRightY = body.Joints[JointType.HipRight].Position.Y; var HipRightZ = ShoulderRightZ;

                                    var HipLeftX = ShoulderLeftX; var HipLeftY = body.Joints[JointType.HipLeft].Position.Y; var HipLeftZ = ShoulderLeftZ;

                                    /*
                                    var ABX = Math.Sqrt(Math.Pow(ShoulderRightZ - ArmRightZ, 2) + Math.Pow(ShoulderRightY - ArmRightY, 2));
                                    var BCX = Math.Sqrt(Math.Pow(ShoulderRightZ - HipRightZ, 2) + Math.Pow(ShoulderRightY - HipRightY, 2));
                                    var ACX = Math.Sqrt(Math.Pow(HipRightZ - ArmRightZ, 2) + Math.Pow(HipRightY - ArmRightY, 2));
                                    var AngleX = Math.Acos((BCX * BCX + ABX * ABX - ACX * ACX) / (2 * BCX * ABX)) * (180 / Math.PI);
                                    */
                                    
                                    var ABY = Math.Sqrt(Math.Pow(ShoulderRightX - ArmRightX, 2) + Math.Pow(ShoulderRightY - ArmRightY, 2));
                                    var BCY = Math.Sqrt(Math.Pow(ShoulderRightX - HipRightX, 2) + Math.Pow(ShoulderRightY - HipRightY, 2));
                                    var ACY = Math.Sqrt(Math.Pow(HipRightX - ArmRightX, 2) + Math.Pow(HipRightY - ArmRightY, 2));
                                    var RightY = Math.Acos((BCY * BCY + ABY * ABY - ACY * ACY) / (2 * BCY * ABY)) * (180 / Math.PI);

                                    ABY = Math.Sqrt(Math.Pow(ShoulderLeftX - ArmLeftX, 2) + Math.Pow(ShoulderLeftY - ArmLeftY, 2));
                                    BCY = Math.Sqrt(Math.Pow(ShoulderLeftX - HipLeftX, 2) + Math.Pow(ShoulderLeftY - HipLeftY, 2));
                                    ACY = Math.Sqrt(Math.Pow(HipLeftX - ArmLeftX, 2) + Math.Pow(HipLeftY - ArmLeftY, 2));
                                    var LeftY = Math.Acos((BCY * BCY + ABY * ABY - ACY * ACY) / (2 * BCY * ABY)) * (180 / Math.PI);


                                    var ABZ = Math.Sqrt(Math.Pow(ShoulderLeftX - ShoulderRightX, 2) + Math.Pow(ShoulderLeftZ - ShoulderRightZ, 2));
                                    var BCZ = Math.Sqrt(Math.Pow(ShoulderRightX - ArmRightX, 2) + Math.Pow(ShoulderRightZ - ArmRightZ, 2));
                                    var ACZ = Math.Sqrt(Math.Pow(ShoulderLeftX - ArmRightX, 2) + Math.Pow(ShoulderLeftZ - ArmRightZ, 2));
                                    var RightZ = Math.Acos((BCZ * BCZ + ABZ * ABZ - ACZ * ACZ) / (2 * BCZ * ABZ))* (180 / Math.PI);

                                    ABZ = Math.Sqrt(Math.Pow(ShoulderRightX - ShoulderLeftX, 2) + Math.Pow(ShoulderRightZ - ShoulderLeftZ, 2));
                                    BCZ = Math.Sqrt(Math.Pow(ShoulderLeftX - ArmLeftX, 2) + Math.Pow(ShoulderLeftZ - ArmLeftZ, 2));
                                    ACZ = Math.Sqrt(Math.Pow(ShoulderRightX - ArmLeftX, 2) + Math.Pow(ShoulderRightZ - ArmLeftZ, 2));
                                    var LeftZ = Math.Acos((BCZ * BCZ + ABZ * ABZ - ACZ * ACZ) / (2 * BCZ * ABZ)) * (180 / Math.PI);

                                    RightZ-= 90;
                                    RightZ = Math.Round(RightZ, 4);
                                    RightY = Math.Round(RightY, 4);
                                    LeftZ -= 90;
                                    LeftZ = Math.Round(LeftZ, 4);
                                    LeftY = Math.Round(LeftY, 4);

                                    Console.WriteLine("/entitydata @e[score_Timer_min=" + y + ",score_Timer=" + y + "] {Pose:{RightArm:[" + -RightY + "f," + RightZ + "f,0f],LeftArm:[" + -LeftY + "f," + -LeftZ + "f,0f]}}");
                                    //Console.WriteLine(ArmRightX + " " + ArmRightY + " | " + ShoulderRightX + " " + ShoulderRightY + " | " + HipRightX + " " + HipRightY);
                                    x = 0;
                                    y++;
                                }
                                if (_mode == CameraMode.Color)
                                {
                                    // Skeleton-to-Color mapping
                                    ColorImagePoint colorPoint = _sensor.CoordinateMapper.MapSkeletonPointToColorPoint(skeletonPoint, ColorImageFormat.RgbResolution640x480Fps30);

                                    point.X = colorPoint.X;
                                    point.Y = colorPoint.Y;
                                }
                                else if (_mode == CameraMode.Depth) // Remember to change the Image and Canvas size to 320x240.
                                {
                                    // Skeleton-to-Depth mapping
                                    DepthImagePoint depthPoint = _sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skeletonPoint, DepthImageFormat.Resolution320x240Fps30);
                                    point.X = depthPoint.X;
                                    point.Y = depthPoint.Y;
                                }

                                // DRAWING...
                                Ellipse ellipse = new Ellipse
                                {
                                    Fill = Brushes.LightBlue,
                                    Width = 20,
                                    Height = 20
                                };

                                Canvas.SetLeft(ellipse, point.X - ellipse.Width / 2);
                                Canvas.SetTop(ellipse, point.Y - ellipse.Height / 2);

                                canvas.Children.Add(ellipse);
                            }
                        }
                    }
                }
            }
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_sensor != null)
            {
                _sensor.Stop();
            }
        }
    }

    enum CameraMode
    {
        Color,
        Depth
    }
}
