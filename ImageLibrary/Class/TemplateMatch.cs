using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogLibrary;
using OpenCvSharp;
using RecipeManager;

namespace ImageLibrary
{
    public class TemplateMatch : IDisposable
    {
        public Log _log = new Log();

        private bool _isLog = false;

        public bool IsLog
        {
            set { _isLog = value; }
            get { return _isLog; }
        }
        public struct MatchResult
        {
            public Rectangle _Rect { get; set; }       // 인식 위치 및 크기 (X, Y, Width, Height)
            public double _Angle { get; set; }    // 회전 변화량 (Degree 단위)
            public double _Score { get; set; }    // 매칭 유사도 (0.0 ~ 1.0)
            public Point2f[] _Corners { get; set; } // 실제 회전된 사각형의 4개 꼭지점 좌표
        }

        public TemplateMatch()
        {

        }

        public void Dispose()
        {

        }

        public bool PatternMatching(Bitmap image, WorkParams workParam, int index)
        {
            Mat search = new Mat(), template = new Mat(), result = new Mat();
            Mat searchPy1 = new Mat(), searchPy2 = new Mat(), searchPy3 = new Mat(), searchPy4 = new Mat();
            Mat templatePy1 = new Mat(), templatePy2 = new Mat(), templatePy3 = new Mat(), templatePy4 = new Mat();
            Mat mark1 = new Mat(), mark2 = new Mat(), ledSearch = new Mat(), matchROI = new Mat();

            float left = 0, right = 0, top = 0, bottom = 0;
            float newLeft = 0, newRight = 0, newTop = 0, newBottom = 0;

            int offset = 15;
            int rotationAngle = 0, similarity = 0;

            int[] templateGradientHistogram = new int[360];
            int[] patternGradientHistogram = new int[360];

            double minval1 = 0.0, maxval1 = 0.0, minval2 = 0.0, maxval2 = 0.0;
            OpenCvSharp.Point minLoc1, minLoc2, maxLoc1, maxLoc2;
            OpenCvSharp.Point originLoc1 = new OpenCvSharp.Point(0, 0), originLoc2 = new OpenCvSharp.Point(0, 0);

            bool IsSearchPyramid = false;            

            Bitmap AlignmentTemplateImage = (Bitmap)Image.FromFile(workParam.MatchingImagePath);

            if (!File.Exists(workParam.MatchingImagePath))
            {
                //_log.WriteLog(LogLevel.Error, LogClass.Image.ToString(), string.Format("Alignement 이미지 경로가 올바르지 않습니다.:{0}", workParam.NearAlignMarkImagePath));
                return false;
            }

            if (AlignmentTemplateImage != null)
            {
                if (!IsSearchPyramid)
                {
                    search = OpenCvSharp.Extensions.BitmapConverter.ToMat(Utils.Clone<Bitmap>(image));

                    // 피라미드 이미지 만들기
                    searchPy1 = search.PyrDown();
                    searchPy2 = searchPy1.PyrDown();
                    searchPy3 = searchPy2.PyrDown();
                    searchPy4 = searchPy3.PyrDown();
                }

                // LED Mark 이미지 
                template = OpenCvSharp.Extensions.BitmapConverter.ToMat(AlignmentTemplateImage);

                // 피라미드 수행
                templatePy1 = template.PyrDown();
                templatePy2 = templatePy1.PyrDown();
                templatePy3 = templatePy2.PyrDown();
                templatePy4 = templatePy3.PyrDown();

                // 3레벨에서 탐색 영역 설정
                // 마크1, 2로 구성된 사각형 설정
                originLoc1 = new OpenCvSharp.Point(workParam.AreaStart.X, workParam.AreaStart.Y);
                originLoc2 = new OpenCvSharp.Point(workParam.AreaEnd.X, workParam.AreaEnd.Y);

                left = (originLoc1.X > originLoc2.X) ? originLoc2.X / 16f : originLoc1.X / 16f;
                right = (originLoc1.X > originLoc2.X) ? originLoc1.X / 16f : originLoc2.X / 16f;
                top = (originLoc1.Y > originLoc2.Y) ? originLoc2.Y / 16f : originLoc1.Y / 16f;
                bottom = (originLoc1.Y > originLoc2.Y) ? originLoc1.Y / 16f : originLoc2.Y / 16f;
                
                // ROI 자르기
                OpenCvSharp.Rect rect = new Rect(Convert.ToInt32(left), Convert.ToInt32(top), Convert.ToInt32(Math.Abs(right - left)), Convert.ToInt32(Math.Abs(bottom - top)));
                ledSearch = searchPy4.SubMat(rect);

                //Cv2.ImShow("Search", ledSearch);

                result = ledSearch.MatchTemplate(templatePy4, TemplateMatchModes.CCoeffNormed);                

                result.MinMaxLoc(out minval1, out maxval1, out minLoc1, out maxLoc1);

                workParam.InspectionPositions[index].Similarity = Convert.ToInt32(maxval1 * 100);

                if (workParam.InspectionPositions[index].Similarity <= workParam.MatchingSimilarityThreshold)
                {
                    _log.WriteLog(LogLevel.Error, LogClass.Image.ToString(), string.Format("Level3의 유사도가 임계치보다 작습니다.({0}<{1})", workParam.InspectionPositions[index].Similarity, workParam.MatchingSimilarityThreshold));

                    search.Dispose();
                    template.Dispose();
                    result.Dispose();
                    searchPy1.Dispose();
                    searchPy2.Dispose();
                    searchPy3.Dispose();
                    searchPy4.Dispose();
                    templatePy1.Dispose();
                    templatePy2.Dispose();
                    templatePy3.Dispose();
                    templatePy4.Dispose();
                    mark1.Dispose();
                    mark2.Dispose();
                    ledSearch.Dispose();
                    matchROI.Dispose();

                    return false;
                }

                // LED 마크의 레벨0 위치 설정
                originLoc1 = new OpenCvSharp.Point((left + maxLoc1.X) * 16, (top + maxLoc1.Y) * 16);

                // 17x17 탐색
                left = (originLoc1.X - offset < 0) ? 0 : originLoc1.X - offset;
                top = (originLoc1.Y - offset < 0) ? 0 : originLoc1.Y - offset;
                right = (originLoc1.X + template.Width + offset + 1 >= search.Width) ? search.Width - 1 : (originLoc1.X + template.Width + offset + 1);
                bottom = (originLoc1.Y + template.Height + offset + 1 >= search.Height) ? search.Height - 1 : (originLoc1.Y + template.Height + offset + 1);

                mark1 = search.SubMat(Convert.ToInt32(top), Convert.ToInt32(bottom), Convert.ToInt32(left), Convert.ToInt32(right));

                result = mark1.MatchTemplate(template, TemplateMatchModes.CCoeffNormed);
                result.MinMaxLoc(out minval1, out maxval1, out minLoc1, out maxLoc1);
                //Cv2.ImShow("Matching", mark1);
                // 회전에 대한 매칭 적용(각도와 유사도 산출)
                //if (workParam.IsLEDRotationCompensation)
                //{
                //templateGradientHistogram = new int[360];
                //patternGradientHistogram = new int[360];

                //newLeft = (maxLoc1.X - 3) < 0 ? 0 : (maxLoc1.X - 3);
                //newTop = (maxLoc1.Y - 3) < 0 ? 0 : (maxLoc1.Y - 3);
                //newRight = (maxLoc1.X + template.Width + 3) > mark1.Width - 1 ? mark1.Width - 1 : (maxLoc1.X + template.Width + 3);
                //newBottom = (maxLoc1.Y + template.Height + 3) > mark1.Height - 1 ? mark1.Height - 1 : (maxLoc1.Y + template.Height + 3);

                //templateGradientHistogram = GetGradientHistogram(template, 1, workParam.LEDCannyEdgeMinThreshold, workParam.LEDCannyEdgeMaxThreshold);

                //rect = new Rect(Convert.ToInt32(newLeft), Convert.ToInt32(newTop), Convert.ToInt32(Math.Abs(newRight - newLeft)), Convert.ToInt32(Math.Abs(newBottom - newTop)));
                //matchROI = mark1.SubMat(rect);

                //patternGradientHistogram = GetGradientHistogram(matchROI, 1, workParam.LEDCannyEdgeMinThreshold, workParam.LEDCannyEdgeMaxThreshold);

                //rotationAngle = 0;
                //similarity = 0;

                //GetRotateAngle(templateGradientHistogram, patternGradientHistogram, 1, ref rotationAngle, ref similarity);

                //workParam.InspectionPositions[index].Similarity = similarity;
                //}
                //else
                //workParam.MatchStart
                {
                    workParam.InspectionPositions[index].Similarity = Convert.ToInt32(maxval1 * 100);
                }

                if (workParam.InspectionPositions[index].Similarity <= workParam.MatchingSimilarityThreshold)
                {
                    _log.WriteLog(LogLevel.Error, LogClass.Image.ToString(), string.Format("레벨0의 유사도가 임계치보다 작습니다.({0}<{1})", workParam.InspectionPositions[index].Similarity, workParam.MatchingSimilarityThreshold));

                    search.Dispose();
                    template.Dispose();
                    result.Dispose();
                    searchPy1.Dispose();
                    searchPy2.Dispose();
                    searchPy3.Dispose();
                    searchPy4.Dispose();
                    templatePy1.Dispose();
                    templatePy2.Dispose();
                    templatePy3.Dispose();
                    templatePy4.Dispose();
                    mark1.Dispose();
                    mark2.Dispose();
                    ledSearch.Dispose();
                    matchROI.Dispose();

                    return false;
                }
                else
                {
                    OpenCvSharp.Point realLoc1 = new OpenCvSharp.Point(left + maxLoc1.X, top + maxLoc1.Y);

                    workParam.Blobs.Add(
                        new Blob(
                            2,
                            realLoc1.X,
                            realLoc1.Y,
                            template.Width,
                            template.Height,
                            0,
                            (realLoc1.X + template.Width / 2),
                            (realLoc1.Y + template.Height / 2)));

                    // LED 마크 위치와 얼라인마크 1, 2의 중심 사이의 거리 계산
                    workParam.AreaCenter = new PointF((realLoc1.X + template.Width / 2f), (realLoc1.Y + template.Height / 2f));

                    workParam.InspectionPositions[index].Distance = (float)Math.Sqrt(Math.Pow(workParam.ImageCenterX - workParam.AreaCenter.X, 2)
                        + Math.Pow(workParam.ImageCenterY - workParam.AreaCenter.Y, 2)); ;

                    _log.WriteLog(LogLevel.Info, LogClass.Image.ToString(), string.Format("패턴매칭 (X:{0}, Y:{1}, 회전각:{2}, 유사도:{3})", workParam.AreaCenter.X, workParam.AreaCenter.Y, rotationAngle, workParam.InspectionPositions[index].Similarity));
                }
            }
            search.Dispose();
            template.Dispose();
            result.Dispose();
            searchPy1.Dispose();
            searchPy2.Dispose();
            searchPy3.Dispose();
            searchPy4.Dispose();
            templatePy1.Dispose();
            templatePy2.Dispose();
            templatePy3.Dispose();
            templatePy4.Dispose();
            mark1.Dispose();
            mark2.Dispose();
            ledSearch.Dispose();
            matchROI.Dispose();
            return true;
        }
        //public bool JigCalibrationPatternMatching(Bitmap image, Bitmap templateimg, int _ithreshold, int _similarity, WorkParams workParam, int index)
        public bool JigCalibrationPatternMatching(Bitmap image, Bitmap templateimg, int _ithreshold, int _similarity)
        {
            try
            {
                Mat source = new Mat(), template = new Mat(), tempimg = new Mat(),result = new Mat();
                Mat sourcegray = new Mat(), templategray = new Mat();
                Mat sourcebinary = new Mat(), templatebinary = new Mat();
                Mat templatePy1 = new Mat(), templatePy2 = new Mat(), templatePy3 = new Mat(), templatePy4 = new Mat();
                Mat mark1 = new Mat(), mark2 = new Mat(), ledSearch = new Mat(), matchROI = new Mat();

                double minval = 0D, maxval = 0D;
                OpenCvSharp.Point minLoc1, minLoc2, maxLoc1, maxLoc2;
                if (templateimg != null)
                {
                    if (image.PixelFormat != PixelFormat.Format8bppIndexed)
                    {
                        source = OpenCvSharp.Extensions.BitmapConverter.ToMat(Utils.Clone<Bitmap>(image));
                        Cv2.CvtColor(source, sourcegray, ColorConversionCodes.BGR2GRAY);
                        Cv2.GaussianBlur(sourcegray, sourcegray, new OpenCvSharp.Size(3, 3), 0);
                        Cv2.Threshold(sourcegray, sourcebinary, _ithreshold, 255 , ThresholdTypes.Binary | ThresholdTypes.Otsu);
                    }
                    else
                    {
                        source = OpenCvSharp.Extensions.BitmapConverter.ToMat(Utils.Clone<Bitmap>(image));
                        sourcegray = source;
                        Cv2.GaussianBlur(sourcegray, sourcegray, new OpenCvSharp.Size(3, 3), 0);
                        Cv2.Threshold(sourcegray, sourcebinary, _ithreshold, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);                        
                    }
                    if (templateimg.PixelFormat != PixelFormat.Format8bppIndexed)
                    {
                        template = OpenCvSharp.Extensions.BitmapConverter.ToMat(Utils.Clone<Bitmap>(templateimg));
                        Cv2.CvtColor(template, templategray, ColorConversionCodes.BGR2GRAY);
                        Cv2.GaussianBlur(templategray, templategray, new OpenCvSharp.Size(3, 3), 0);
                        Cv2.Threshold(templategray, templategray, 0, _ithreshold, ThresholdTypes.Binary | ThresholdTypes.Otsu);
                    }
                    else
                    {
                        template = OpenCvSharp.Extensions.BitmapConverter.ToMat(Utils.Clone<Bitmap>(templateimg));
                        templategray = template;
                        Cv2.GaussianBlur(templategray, templategray, new OpenCvSharp.Size(3, 3), 0);
                        Cv2.Threshold(templategray, templatebinary, _ithreshold, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);                        
                    }
                    //Cv2.ImShow("gray Results", sourcegray);
                    //Cv2.ImShow("source binary Results", sourcebinary);
                    //Cv2.ImShow("binary Results", templatebinary);
                    Cv2.MatchTemplate(sourcebinary, templatebinary, result,TemplateMatchModes.CCoeffNormed);

                    double similarthreshold = 0;
                    List<Rect> detectedObjects = new List<Rect>();

                    var indexer = result.GetGenericIndexer<float>();

                    similarthreshold = (_similarity / 100D);
                    for (int y = 0; y < result.Rows; y++)
                    {
                        for (int x = 0; x < result.Cols; x++)
                        {
                            float matchValue = indexer[y, x];

                            // 임계값 조건을 충족하는 경우 위치와 크기 저장
                            if (matchValue >= similarthreshold)
                            {
                                // 크기는 항상 찾으려는 템플릿 이미지의 크기와 같습니다.
                                Rect rect = new Rect(x, y, template.Width, template.Height);
                                detectedObjects.Add(rect);
                            }
                        }
                    }
                    // 중복 검출 영역 제거 (NMS - Non-Maximum Suppression 개념 적용)
                    // 패턴 주위로 픽셀 단위의 미세하게 겹치는 사각형들이 무수히 생기므로 이를 하나로 병합합니다.
                    List<Rect> filteredObjects = FilterGroupRectangles(detectedObjects, groupThreshold: 1, eps: 0.2);

                    // 6. 결과 출력 및 화면 사각형 표시
                    Console.WriteLine($"총 {filteredObjects.Count}개의 객체를 발견했습니다.\n");

                    for (int i = 0; i < filteredObjects.Count; i++)
                    {
                        Rect obj = filteredObjects[i];

                        // 크기와 위치 정보 출력
                        Console.WriteLine($"[객체 {i + 1}] 위치: X={obj.X}, Y={obj.Y} | 크기: W={obj.Width}, H={obj.Height}");

                        // 원본 영상에 시각화 (녹색 사각형)
                        Cv2.Rectangle(sourcegray, obj, Scalar.FromRgb(0, 255, 0), 2);

                        // 사각형 위에 번호 텍스트 쓰기
                        Cv2.PutText(sourcegray, (i + 1).ToString(), new OpenCvSharp.Point(obj.X, obj.Y - 5),
                                    HersheyFonts.HersheySimplex, 0.6, Scalar.FromRgb(0, 255, 0), 2);
                    }
                    Cv2.ImShow("Multi-Pattern Results", sourcegray);
                    Cv2.WaitKey(0);
                }
                else
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }            
        }

        public List<MatchResult> MultiPatternMatching(Bitmap image, Bitmap templateimg, int _ithreshold, int _similarity)
        {
            try
            {
                List<MatchResult> finalResults = new List<MatchResult>();

                Mat source = new Mat(), template = new Mat(), tempimg = new Mat(), result = new Mat();
                Mat sourcegray = new Mat(), templategray = new Mat();
                Mat sourcebinary = new Mat(), templatebinary = new Mat();

                if (templateimg != null)
                {
                    if (image.PixelFormat != PixelFormat.Format8bppIndexed)
                    {
                        //source = OpenCvSharp.Extensions.BitmapConverter.ToMat(Utils.Clone<Bitmap>(image));
                        source = OpenCvSharp.Extensions.BitmapConverter.ToMat(image);
                        Cv2.CvtColor(source, sourcegray, ColorConversionCodes.BGR2GRAY);
                        Cv2.GaussianBlur(sourcegray, sourcegray, new OpenCvSharp.Size(3, 3), 0);
                        Cv2.Threshold(sourcegray, sourcebinary, _ithreshold, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);
                    }
                    else
                    {
                        //source = OpenCvSharp.Extensions.BitmapConverter.ToMat(Utils.Clone<Bitmap>(image));
                        source = OpenCvSharp.Extensions.BitmapConverter.ToMat(image);
                        sourcegray = source;
                        Cv2.GaussianBlur(sourcegray, sourcegray, new OpenCvSharp.Size(3, 3), 0);
                        Cv2.Threshold(sourcegray, sourcebinary, _ithreshold, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);
                    }
                    if (templateimg.PixelFormat != PixelFormat.Format8bppIndexed)
                    {
                        //template = OpenCvSharp.Extensions.BitmapConverter.ToMat(Utils.Clone<Bitmap>(templateimg));
                        template = OpenCvSharp.Extensions.BitmapConverter.ToMat(templateimg);
                        Cv2.CvtColor(template, templategray, ColorConversionCodes.BGR2GRAY);
                        Cv2.GaussianBlur(templategray, templategray, new OpenCvSharp.Size(3, 3), 0);
                        Cv2.Threshold(templategray, templategray, 0, _ithreshold, ThresholdTypes.Binary | ThresholdTypes.Otsu);
                    }
                    else
                    {
                        //template = OpenCvSharp.Extensions.BitmapConverter.ToMat(Utils.Clone<Bitmap>(templateimg));
                        template = OpenCvSharp.Extensions.BitmapConverter.ToMat(templateimg);
                        templategray = template;
                        Cv2.GaussianBlur(templategray, templategray, new OpenCvSharp.Size(3, 3), 0);
                        Cv2.Threshold(templategray, templatebinary, _ithreshold, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);
                    }
                    //Cv2.ImShow("gray Results", sourcegray);
                    //Cv2.ImShow("source binary Results", sourcebinary);
                    //Cv2.ImShow("binary Results", templatebinary);
                    Cv2.MatchTemplate(sourcebinary, templatebinary, result, TemplateMatchModes.CCoeffNormed);

                    double similarthreshold = 0;
                    List<Rect> detectedObjects = new List<Rect>();

                    var indexer = result.GetGenericIndexer<float>();

                    similarthreshold = (_similarity / 100D);
                    for (int y = 0; y < result.Rows; y++)
                    {
                        for (int x = 0; x < result.Cols; x++)
                        {
                            float matchValue = indexer[y, x];

                            // 임계값 조건을 충족하는 경우 위치와 크기 저장
                            if (matchValue >= similarthreshold)
                            {
                                // 크기는 항상 찾으려는 템플릿 이미지의 크기와 같습니다.
                                Rect rect = new Rect(x, y, template.Width, template.Height);
                                detectedObjects.Add(rect);
                            }
                        }
                    }
                    // 중복 검출 영역 제거 (NMS - Non-Maximum Suppression 개념 적용)
                    // 패턴 주위로 픽셀 단위의 미세하게 겹치는 사각형들이 무수히 생기므로 이를 하나로 병합합니다.
                    List<Rect> filteredObjects = FilterGroupRectangles(detectedObjects, groupThreshold: 1, eps: 0.2);

                    // 6. 결과 출력 및 화면 사각형 표시
                    Console.WriteLine($"총 {filteredObjects.Count}개의 객체를 발견했습니다.\n");

                    for (int i = 0; i < filteredObjects.Count; i++)
                    {
                        Rect obj = filteredObjects[i];
                        Rectangle rectobj = new Rectangle(obj.X,obj.Y,obj.Width,obj.Height);
                        // 크기와 위치 정보 출력
                        Console.WriteLine($"[객체 {i + 1}] 위치: X={obj.X}, Y={obj.Y} | 크기: W={obj.Width}, H={obj.Height}");

                        // 원본 영상에 시각화 (녹색 사각형)
                        Cv2.Rectangle(sourcegray, obj, Scalar.FromRgb(0, 255, 0), 2);

                        // 사각형 위에 번호 텍스트 쓰기
                        Cv2.PutText(sourcegray, (i + 1).ToString(), new OpenCvSharp.Point(obj.X, obj.Y - 5),
                                    HersheyFonts.HersheySimplex, 0.6, Scalar.FromRgb(0, 255, 0), 2);
                        finalResults.Add(new MatchResult
                        {
                            _Rect = rectobj,                     // 대략적인 감싸는 사각형 영역 위치
                            _Angle = 0,                      // 단 한 번에 알아낸 정밀 회전량
                            _Score = 0,                      // 매칭된 점의 개수를 신뢰도 점수로 활용
                            //Corners = new OpenCvSharp.Point2f()                     // 실제 찌그러지고 회전된 형태의 4점 좌표
                        });
                    }
                    //Cv2.ImShow("Multi-Pattern Results", sourcegray);
                    //Cv2.WaitKey(0);
                }
                else
                    return finalResults;

                return finalResults;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public List<MatchResult> DetectMultiRotatedPatterns(Bitmap image, Bitmap templateimg, int _ithreshold, int _similarity)
        {
            try
            {
                List<MatchResult> finalResults = new List<MatchResult>();

                Mat source = new Mat(), template = new Mat(), tempimg = new Mat(), result = new Mat();
                Mat sourcegray = new Mat(), templategray = new Mat();
                Mat sourcebinary = new Mat(), templatebinary = new Mat();           
                
                if (templateimg != null)
                {
                    if (image.PixelFormat != PixelFormat.Format8bppIndexed)
                    {
                        source = OpenCvSharp.Extensions.BitmapConverter.ToMat(Utils.Clone<Bitmap>(image));
                        Cv2.CvtColor(source, sourcegray, ColorConversionCodes.BGR2GRAY);
                        Cv2.GaussianBlur(sourcegray, sourcegray, new OpenCvSharp.Size(3, 3), 0);
                        Cv2.Threshold(sourcegray, sourcebinary, _ithreshold, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);
                    }
                    else
                    {
                        source = OpenCvSharp.Extensions.BitmapConverter.ToMat(Utils.Clone<Bitmap>(image));
                        sourcegray = source;
                        Cv2.GaussianBlur(sourcegray, sourcegray, new OpenCvSharp.Size(3, 3), 0);
                        Cv2.Threshold(sourcegray, sourcebinary, _ithreshold, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);
                    }
                    if (templateimg.PixelFormat != PixelFormat.Format8bppIndexed)
                    {
                        template = OpenCvSharp.Extensions.BitmapConverter.ToMat(Utils.Clone<Bitmap>(templateimg));
                        Cv2.CvtColor(template, templategray, ColorConversionCodes.BGR2GRAY);
                        Cv2.GaussianBlur(templategray, templategray, new OpenCvSharp.Size(3, 3), 0);
                        Cv2.Threshold(templategray, templategray, 0, _ithreshold, ThresholdTypes.Binary | ThresholdTypes.Otsu);
                    }
                    else
                    {
                        template = OpenCvSharp.Extensions.BitmapConverter.ToMat(Utils.Clone<Bitmap>(templateimg));
                        templategray = template;
                        Cv2.GaussianBlur(templategray, templategray, new OpenCvSharp.Size(3, 3), 0);
                        Cv2.Threshold(templategray, templatebinary, _ithreshold, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);
                    }                    
                    //Cv2.ImShow("source binary Results", sourcebinary);
                    //Cv2.ImShow("binary Results", templatebinary);
                    //Cv2.MatchTemplate(sourcebinary, templatebinary, result, TemplateMatchModes.CCoeffNormed);

                    using (ORB orb = ORB.Create(nFeatures: 2000))
                    using (Mat descriptorsSrc = new Mat())
                    using (Mat descriptorsTemplate = new Mat())
                    {
                        KeyPoint[] keypointsSrc, keypointsTemplate;

                        //두 이미지에서 각각 특징점과 기술자 추출
                        orb.DetectAndCompute(sourcegray, null, out keypointsSrc, descriptorsSrc);
                        orb.DetectAndCompute(templategray, null, out keypointsTemplate, descriptorsTemplate);

                        if (descriptorsSrc.Empty() || descriptorsTemplate.Empty()) return finalResults;
                        //Brute-Force 매칭 처리 (ORB는 이진 기술자를 쓰므로 Hamming 거리를 사용합니다)
                        using (BFMatcher matcher = new BFMatcher(NormTypes.Hamming, crossCheck: true))                        
                        {                            
                            DMatch[] matches = matcher.Match(descriptorsTemplate, descriptorsSrc);
                            if (matches.Length < 4) return finalResults;                            // 호모그래피 생성에는 최소 4개의 매칭점이 필요합니다.

                            //매칭 결과 정렬 및 좋은 매칭점(Good Matches) 필터링
                            Array.Sort(matches, (a, b) => a.Distance.CompareTo(b.Distance));
                            List<DMatch> goodMatches = new List<DMatch>();
                            // 상위 우수한 특징점 60개만 선택 (상황에 따라 조절)
                            int takeCount = Math.Min(60, matches.Length);
                            for (int i = 0; i < takeCount; i++)
                            {
                                // 거리값이 너무 큰 불량 매칭 데이터는 사전에 필터링 (Hamming 거리 50 이하 권장)
                                if (matches[i].Distance < 50)
                                    goodMatches.Add(matches[i]);
                            }
                            if (goodMatches.Count < 4) return finalResults;
                            // 회전된 사각형 영역 계산 (호모그래피 - Homography 활용)
                            List<Point2f> srcPoints = new List<Point2f>();
                            List<Point2f> dstPoints = new List<Point2f>();

                            foreach (var match in goodMatches)
                            {
                                srcPoints.Add(keypointsTemplate[match.QueryIdx].Pt);
                                dstPoints.Add(keypointsSrc[match.TrainIdx].Pt);
                            }                          
                            
                            // 특징점들을 바탕으로 투영 변환 행렬(Homography) 계산
                            using (Mat homography = Cv2.FindHomography(InputArray.Create(srcPoints), InputArray.Create(dstPoints), HomographyMethods.Ransac))
                            {
                                if (!homography.Empty())
                                {
                                    double m00 = homography.At<double>(0, 0);
                                    double m10 = homography.At<double>(1, 0);
                                    double radian = Math.Atan2(m10, m00);
                                    double calculatedAngle = radian * (180.0 / Math.PI);
                                    // 패턴의 원래 모서리 4점 정의
                                    Point2f[] templateCorners = new Point2f[]
                                    {
                                        new Point2f(0, 0),
                                        new Point2f(templatebinary.Width, 0),
                                        new Point2f(templatebinary.Width, templatebinary.Height),
                                        new Point2f(0, templatebinary.Height)
                                    };

                                    // 변환 행렬을 적용하여 원본 이미지 상의 회전된 4점 좌표 계산
                                    Point2f[] sceneCorners = Cv2.PerspectiveTransform(templateCorners, homography);

                                    // 9. 결과 데이터 래핑 및 Bounds Rect 연산
                                    RotatedRect rRect = Cv2.MinAreaRect(sceneCorners);                                    
                                    Rectangle rectobj = new Rectangle(rRect.BoundingRect().X, rRect.BoundingRect().Y, rRect.BoundingRect().Width, rRect.BoundingRect().Height);
                                    finalResults.Add(new MatchResult
                                    {
                                        _Rect = rectobj, // 대략적인 감싸는 사각형 영역 위치
                                        _Angle = calculatedAngle,    // 단 한 번에 알아낸 정밀 회전량
                                        _Score = goodMatches.Count,   // 매칭된 점의 개수를 신뢰도 점수로 활용
                                        _Corners = sceneCorners       // 실제 찌그러지고 회전된 형태의 4점 좌표
                                    });
                                    Console.WriteLine("회전된 패턴을 성공적으로 검출했습니다.");
                                }
                            }
                            

                            // 결과 확인 (두 이미지의 매칭 관계를 선으로 보여주는 창)
                            //using (Mat displayimg = new Mat())
                            {
                                //displayimg = sourcegray;
                                for (int i = 0; i < finalResults.Count; i++)
                                {
                                    MatchResult item = finalResults[i];
                                    Console.WriteLine($"[객체 {i + 1}]");
                                    Console.WriteLine($" -> 위치(중심점 부근): X={item._Rect.X}, Y={item._Rect.Y}");
                                    Console.WriteLine($" -> 회전 변화량: {item._Angle:F2}°");
                                    Console.WriteLine($" -> 신뢰도(매칭점 개수): {item._Score}개\n");

                                    // 실제 회전된 사각형의 4개 꼭지점을 연결하여 초록색 선으로 시각화
                                    for (int j = 0; j < 4; j++)
                                    {
                                        Cv2.Line(sourcegray, (OpenCvSharp.Point)item._Corners[j], (OpenCvSharp.Point)item._Corners[(j + 1) % 4], Scalar.FromRgb(0, 255, 0), 2);
                                    }

                                    // 텍스트 정보 표시 (번호 및 각도)
                                    Cv2.PutText(sourcegray, $"#{i + 1} ({item._Angle:F1}°)", (OpenCvSharp.Point)item._Corners[0],
                                                HersheyFonts.HersheySimplex, 0.5, Scalar.FromRgb(0, 255, 0), 2);
                                }

                                // 결과 창 출력
                                Cv2.ImShow("Non-Loop Rotated Matching Results", sourcegray);
                                Cv2.WaitKey(0);
                            }
                        }
                    }
                }
                return finalResults;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private static List<Rect> FilterGroupRectangles(List<Rect> rects, int groupThreshold, double eps)
        {
            List<Rect> result = new List<Rect>(rects);
            if (result.Count == 0) return result;

            // OpenCV의 groupRectangles 함수를 활용해 미세하게 겹친 영역을 병합합니다.
            Cv2.GroupRectangles(result, groupThreshold, eps);
            return result;
        }
        public bool OpticalSpotProcess(Bitmap image, WorkParams workParam, int index)
        {
            /*
            ///////////////////////////////////////////////////////////////////////////////////
            /// LED 위치 찾기
            Mat search = new Mat(), searchPy1 = new Mat(), searchPy2 = new Mat(), searchPy3 = new Mat();
            Mat template = new Mat(), templatePy1 = new Mat(), templatePy2 = new Mat(), templatePy3 = new Mat();
            Mat subRegion = new Mat();
            Mat result = new Mat();

            string strFilePath = (workParam.PatternSelectionMode == PATTERN_SELECTION_MODE.PATTERN_SELECTION_MODE_NEAR) ? workParam.NearAlignMarkImagePath : workParam.FarAlignMarkImagePath;
            string strPattern = (workParam.PatternSelectionMode == PATTERN_SELECTION_MODE.PATTERN_SELECTION_MODE_NEAR) ? "시작패턴" : "끝패턴";

            int similarity = 0;
            int similarityThreshold = (workParam.PatternSelectionMode == PATTERN_SELECTION_MODE.PATTERN_SELECTION_MODE_NEAR) ? workParam.NearAlignmentSimilarityThreshold : workParam.FarAlignmentSimilarityThreshold;

            PointF fptPosition = new PointF(-100f, -100f);

            if (!File.Exists(strFilePath))
            {
                //_log.WriteLog(LogLevel.Error, LogClass.Image.ToString(), string.Format("{0} 이미지 경로가 올바르지 않습니다.:{1}", strPattern, strFilePath));
                return false;
            }

            Bitmap templateImage = (Bitmap)Image.FromFile(strFilePath);

            if (templateImage != null)
            {
                OpenCvSharp.Point originLoc = new OpenCvSharp.Point();
                OpenCvSharp.Point minLoc = new OpenCvSharp.Point(), maxLoc = new OpenCvSharp.Point();

                float left = 0, top = 0, right = 0, bottom = 0;
                int offset = 12;

                double minval = 0, maxval = 0;


                search = OpenCvSharp.Extensions.BitmapConverter.ToMat(Utils.Clone<Bitmap>(image));

                // 피라미드 이미지 만들기
                searchPy1 = search.PyrDown();
                searchPy2 = searchPy1.PyrDown();
                searchPy3 = searchPy2.PyrDown();

                //OpenCvSharp.Rect mark1 = new OpenCvSharp.Rect(553, 1053, 625, 625);
                //Mat temp1 = search.SubMat(mark1);
                //OpenCvSharp.Extensions.BitmapConverter.ToBitmap(temp1).Save(@"D:\PCB_Align.bmp", ImageFormat.Bmp);

                // PCB Align Mark 이미지 
                template = OpenCvSharp.Extensions.BitmapConverter.ToMat(Utils.Clone<Bitmap>(templateImage));

                // 피라미드 수행
                templatePy1 = template.PyrDown();
                templatePy2 = templatePy1.PyrDown();
                templatePy3 = templatePy2.PyrDown();

                // 피라미드 검증용 영상
                OpenCvSharp.Extensions.BitmapConverter.ToBitmap(searchPy3).Save(@"F:\Image\searchPy3.bmp", ImageFormat.Bmp);
                //OpenCvSharp.Extensions.BitmapConverter.ToBitmap(templatePy1).Save(@"F:\Image\LedtemplatePy1.bmp", ImageFormat.Bmp);
                //OpenCvSharp.Extensions.BitmapConverter.ToBitmap(templatePy2).Save(@"F:\Image\LedtemplatePy2.bmp", ImageFormat.Bmp);
                OpenCvSharp.Extensions.BitmapConverter.ToBitmap(templatePy3).Save(@"F:\Image\LedtemplatePy3.bmp", ImageFormat.Bmp);

                result = searchPy3.MatchTemplate(templatePy3, TemplateMatchModes.CCoeffNormed);
                result.MinMaxLoc(out minval, out maxval, out minLoc, out maxLoc);

                similarity = Convert.ToInt32(maxval * 100);
                workParam.InspectionPositions[index].Similarity3 = similarity;

                if (similarity <= similarityThreshold)
                {
                    //_log.WriteLog(LogLevel.Error, LogClass.Image.ToString(), string.Format("레벨3의 {0} 유사도가 임계치보다 작습니다.({1}<{2})", strPattern, similarity, similarityThreshold));

                    search.Dispose();
                    template.Dispose();
                    result.Dispose();
                    searchPy1.Dispose();
                    searchPy2.Dispose();
                    templatePy1.Dispose();
                    templatePy2.Dispose();

                    return false;
                }

                // LED 마크의 레벨0 위치 설정
                originLoc = new OpenCvSharp.Point((left + maxLoc.X) * 8, (top + maxLoc.Y) * 8);

                // 17x17 탐색
                left = (originLoc.X - offset < 0) ? 0 : originLoc.X - offset;
                top = (originLoc.Y - offset < 0) ? 0 : originLoc.Y - offset;
                right = (originLoc.X + template.Width + offset + 1 >= search.Width) ? search.Width - 1 : (originLoc.X + template.Width + offset + 1);
                bottom = (originLoc.Y + template.Height + offset + 1 >= search.Height) ? search.Height - 1 : (originLoc.Y + template.Height + offset + 1);

                subRegion = search.SubMat(Convert.ToInt32(top), Convert.ToInt32(bottom), Convert.ToInt32(left), Convert.ToInt32(right));

                result = subRegion.MatchTemplate(template, TemplateMatchModes.CCoeffNormed);
                result.MinMaxLoc(out minval, out maxval, out minLoc, out maxLoc);

                //OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mark1).Save(@"F:\Image\originLEDMark.bmp", ImageFormat.Bmp);

                similarity = Convert.ToInt32(maxval * 100);
                workParam.InspectionPositions[index].Similarity1 = similarity;

                if (similarity <= similarityThreshold)
                {
                    //_log.WriteLog(LogLevel.Error, LogClass.Image.ToString(), string.Format("레벨0의 {0} 유사도가 임계치보다 작습니다.({1}<{2})", strPattern, similarity, similarityThreshold));

                    search.Dispose();
                    template.Dispose();
                    result.Dispose();
                    searchPy1.Dispose();
                    searchPy2.Dispose();
                    templatePy1.Dispose();
                    templatePy2.Dispose();
                    subRegion.Dispose();

                    return false;
                }
                else
                {
                    fptPosition = new PointF(left + maxLoc.X + template.Width / 2f, top + maxLoc.Y + template.Height / 2f);

                    if (workParam.PatternSelectionMode == PATTERN_SELECTION_MODE.PATTERN_SELECTION_MODE_NEAR)
                        workParam.NearAlignPosition = fptPosition;
                    else
                        workParam.FarAlignPosition = fptPosition;

                    //_log.WriteLog(LogLevel.Info, LogClass.Image.ToString(), string.Format("{0} (X:{1}, Y:{2}, 유사도:{3})", strPattern, fptPosition.X, fptPosition.Y, similarity));
                }
            }

            // Free Memory
            search.Dispose();
            template.Dispose();
            result.Dispose();
            searchPy1.Dispose();
            searchPy2.Dispose();
            templatePy1.Dispose();
            templatePy2.Dispose();
            subRegion.Dispose();
            */
            return true;
        }
        private void GetRotateAngle(int[] templateGradientHistogram, int[] patternGradientHistogram, int scale, ref int rotateAngle, ref int similarity)
        {
            rotateAngle = 0;
            similarity = 0;

            float minValue = float.MinValue;

            int length = 360 / scale;

            int range = Convert.ToInt32(length * 0.1);

            float squareTemplate = 0;
            float squarePattern = 0;

            for (int i = 0; i < length; ++i)
            {
                squareTemplate += (templateGradientHistogram[i] * templateGradientHistogram[i]);
                squarePattern += (patternGradientHistogram[i] * patternGradientHistogram[i]);
            }

            for (int angle = 0; angle < range; ++angle)
            {
                float inner = 0;
                float coe = 0;

                for (int g = 0; g < length; ++g)
                {
                    int tempIndex = (angle + g) % length;

                    inner += (templateGradientHistogram[tempIndex] * patternGradientHistogram[g]);
                }

                coe = inner * inner / (squareTemplate * squarePattern);

                if (!float.IsNaN(coe))
                {
                    if (coe > minValue)
                    {
                        minValue = coe;
                        rotateAngle = angle * scale;
                    }
                }
            }

            if (minValue != float.MinValue)
            {
                for (int angle = length - range; angle < length; ++angle)
                {
                    float inner = 0;
                    float coe = 0;

                    for (int g = 0; g < length; ++g)
                    {
                        int tempIndex = (angle + g) % length;

                        inner += (templateGradientHistogram[tempIndex] * patternGradientHistogram[g]);
                    }

                    coe = inner * inner / (squareTemplate * squarePattern);

                    if (coe > minValue)
                    {
                        minValue = coe;
                        rotateAngle = angle * scale;
                    }
                }

                similarity = Convert.ToInt32(minValue * 100);
            }
            else
                similarity = 0;
        }

        unsafe private int[] GetGradientHistogram(Mat image, int scale, int minThreshold, int maxThreshold)
        {
            int length = 360 / scale;
            int[] histogram = new int[length];

            Mat blur = image.Blur(new OpenCvSharp.Size(3, 3));
            Mat canny = blur.Canny(minThreshold, maxThreshold);

            int index1, index2;
            int gx, gy;
            int stride = (int)canny.Step();
            byte* pSearch = (byte*)canny.Data;
            byte* pblur = (byte*)blur.Data;

            for (int y = 1; y < canny.Rows - 1; ++y)
            {
                index1 = (int)(y * stride);

                for (int x = 1; x < canny.Cols - 1; ++x)
                {
                    index2 = index1 + x;

                    if (pSearch[index2] > 0)
                    {
                        gx = pblur[index2 - stride + 1] + (pblur[index2 + 1] * 2) + pblur[index2 + stride + 1]
                            - pblur[index2 - stride - 1] - (pblur[index2 - 1] * 2) - pblur[index2 + stride - 1];

                        gy = pblur[index2 + stride - 1] + (pblur[index2 + stride] * 2) + pblur[index2 + stride + 1]
                            - pblur[index2 - stride - 1] - (pblur[index2 - stride] * 2) - pblur[index2 - stride + 1];

                        int angle = ATan2(gy, gx);

                        if (angle < 0)
                        {
                            angle += 360;
                        }

                        angle = 360 - angle;

                        if (angle == 360)
                            angle = 0;

                        histogram[angle / scale]++;
                    }
                }
            }

            blur.Dispose();
            canny.Dispose();

            return histogram;
        }

        private int ATan2(int y_fp, int x_fp)
        {
            const int MULTIPLY_FP_RESOLUTION_BITS = 15;

            int coeff_1 = 45;
            int coeff_1b = -56;	// 56.24;
            int coeff_1c = 11;	// 11.25
            int coeff_2 = 135;

            int angle = 0;

            int r;
            int r3;

            int y_abs_fp = y_fp;
            if (y_abs_fp < 0)
                y_abs_fp = -y_abs_fp;

            if (y_fp == 0)
            {
                if (x_fp >= 0)
                {
                    angle = 0;
                }
                else
                {
                    angle = 180;
                }
            }
            else if (x_fp >= 0)
            {
                r = (((int)(x_fp - y_abs_fp)) << MULTIPLY_FP_RESOLUTION_BITS) / ((int)(x_fp + y_abs_fp));

                r3 = r * r;
                r3 = r3 >> MULTIPLY_FP_RESOLUTION_BITS;
                r3 *= r;
                r3 = r3 >> MULTIPLY_FP_RESOLUTION_BITS;
                r3 *= coeff_1c;
                angle = (int)(coeff_1 + ((coeff_1b * r + r3) >> MULTIPLY_FP_RESOLUTION_BITS));
            }
            else
            {
                r = (((int)(x_fp + y_abs_fp)) << MULTIPLY_FP_RESOLUTION_BITS) / ((int)(y_abs_fp - x_fp));
                r3 = r * r;
                r3 = r3 >> MULTIPLY_FP_RESOLUTION_BITS;
                r3 *= r;
                r3 = r3 >> MULTIPLY_FP_RESOLUTION_BITS;
                r3 *= coeff_1c;
                angle = coeff_2 + ((int)(((coeff_1b * r + r3) >> MULTIPLY_FP_RESOLUTION_BITS)));
            }

            if (y_fp < 0)
                return (-angle);     // negate if in quad III or IV
            else
                return (angle);

        }
        //public bool PCBProcess(Bitmap image, WorkParams workParam, int index)
        //{
        //    ///////////////////////////////////////////////////////////////////////////////////
        //    /// LED 위치 찾기
        //    Mat search = new Mat(), searchPy1 = new Mat(), searchPy2 = new Mat(), searchPy3 = new Mat();
        //    Mat template = new Mat(), templatePy1 = new Mat(), templatePy2 = new Mat(), templatePy3 = new Mat();
        //    Mat subRegion = new Mat();
        //    Mat result = new Mat();

        //    if (!File.Exists(workParam.NearAlignMarkImagePath))
        //    {
        //        _log.WriteLog(LogLevel.Error, LogClass.Image.ToString(), string.Format("패턴1 이미지 경로가 올바르지 않습니다.:{0}", workParam.PCBAlignMarkImagePath));
        //        return false;
        //    }

        //    Bitmap pcbAlignMarkImage = (Bitmap)Image.FromFile(workParam.NearAlignMarkImagePath);

        //    if (pcbAlignMarkImage != null)
        //    {
        //        OpenCvSharp.Point originLoc = new OpenCvSharp.Point();
        //        OpenCvSharp.Point minLoc = new OpenCvSharp.Point(), maxLoc = new OpenCvSharp.Point();

        //        float left = 0, top = 0, right = 0, bottom = 0;
        //        int offset = 12;

        //        double minval = 0, maxval = 0;


        //        search = OpenCvSharp.Extensions.BitmapConverter.ToMat(Utils.Clone<Bitmap>(image));

        //        // 피라미드 이미지 만들기
        //        searchPy1 = search.PyrDown();
        //        searchPy2 = searchPy1.PyrDown();
        //        searchPy3 = searchPy2.PyrDown();

        //        //OpenCvSharp.Rect mark1 = new OpenCvSharp.Rect(553, 1053, 625, 625);
        //        //Mat temp1 = search.SubMat(mark1);
        //        //OpenCvSharp.Extensions.BitmapConverter.ToBitmap(temp1).Save(@"D:\PCB_Align.bmp", ImageFormat.Bmp);

        //        // PCB Align Mark 이미지 
        //        template = OpenCvSharp.Extensions.BitmapConverter.ToMat(Utils.Clone < Bitmap >(pcbAlignMarkImage));

        //        // 피라미드 수행
        //        templatePy1 = template.PyrDown();
        //        templatePy2 = templatePy1.PyrDown();
        //        templatePy3 = templatePy2.PyrDown();

        //        // 피라미드 검증용 영상
        //        //OpenCvSharp.Extensions.BitmapConverter.ToBitmap(searchPy2).Save(@"F:\Image\searchPy2.bmp", ImageFormat.Bmp);
        //        //OpenCvSharp.Extensions.BitmapConverter.ToBitmap(templatePy1).Save(@"F:\Image\LedtemplatePy1.bmp", ImageFormat.Bmp);
        //        //OpenCvSharp.Extensions.BitmapConverter.ToBitmap(templatePy2).Save(@"F:\Image\LedtemplatePy2.bmp", ImageFormat.Bmp);

        //        result = searchPy3.MatchTemplate(templatePy3, TemplateMatchModes.CCoeffNormed);
        //        result.MinMaxLoc(out minval, out maxval, out minLoc, out maxLoc);

        //        workParam.InspectionPositions[index].Similarity = Convert.ToInt32(maxval * 100);

        //        if (maxval * 100 <= workParam.PCBAlignmentSimilarityThreshold)
        //        {
        //            _log.WriteLog(LogLevel.Error, LogClass.Image.ToString(), string.Format("레벨3의 PCB Alignment 유사도가 임계치보다 작습니다.({0}<{1})", (int)(maxval * 100), workParam.PCBAlignmentSimilarityThreshold));

        //            search.Dispose();
        //            template.Dispose();
        //            result.Dispose();
        //            searchPy1.Dispose();
        //            searchPy2.Dispose();
        //            templatePy1.Dispose();
        //            templatePy2.Dispose();

        //            return false;
        //        }

        //        // LED 마크의 레벨0 위치 설정
        //        originLoc = new OpenCvSharp.Point((left + maxLoc.X) * 8, (top + maxLoc.Y) * 8);

        //        // 17x17 탐색
        //        left = (originLoc.X - offset < 0) ? 0 : originLoc.X - offset;
        //        top = (originLoc.Y - offset < 0) ? 0 : originLoc.Y - offset;
        //        right = (originLoc.X + template.Width + offset + 1 >= search.Width) ? search.Width - 1 : (originLoc.X + template.Width + offset + 1);
        //        bottom = (originLoc.Y + template.Height + offset + 1 >= search.Height) ? search.Height - 1 : (originLoc.Y + template.Height + offset + 1);

        //        subRegion = search.SubMat(Convert.ToInt32(top), Convert.ToInt32(bottom), Convert.ToInt32(left), Convert.ToInt32(right));

        //        result = subRegion.MatchTemplate(template, TemplateMatchModes.CCoeffNormed);
        //        result.MinMaxLoc(out minval, out maxval, out minLoc, out maxLoc);

        //        //OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mark1).Save(@"F:\Image\originLEDMark.bmp", ImageFormat.Bmp);

        //        workParam.InspectionPositions[index].Similarity = Convert.ToInt32(maxval * 100);

        //        if (maxval * 100 <= workParam.PCBAlignmentSimilarityThreshold)
        //        {
        //            _log.WriteLog(LogLevel.Error, LogClass.Image.ToString(), string.Format("레벨0의 PCB Alignment 유사도가 임계치보다 작습니다.({0}<{1})", (int)(maxval * 100), workParam.LEDAlignmentSimilarityThreshold));

        //            search.Dispose();
        //            template.Dispose();
        //            result.Dispose();
        //            searchPy1.Dispose();
        //            searchPy2.Dispose();
        //            templatePy1.Dispose();
        //            templatePy2.Dispose();
        //            subRegion.Dispose();

        //            return false;
        //        }
        //        else
        //        {
        //            workParam.PCBAlignMark1 = new PointF(left + maxLoc.X + template.Width / 2f, top + maxLoc.Y + template.Height / 2f);

        //            _log.WriteLog(LogLevel.Info, LogClass.Image.ToString(), string.Format("PCB (X:{0}, Y:{1}, 유사도:{2})", workParam.PCBAlignMark1.X, workParam.PCBAlignMark1.Y, (int)(maxval * 100)));
        //        }
        //    }

        //    // Free Memory
        //    search.Dispose();
        //    template.Dispose();
        //    result.Dispose();
        //    searchPy1.Dispose();
        //    searchPy2.Dispose();
        //    templatePy1.Dispose();
        //    templatePy2.Dispose();
        //    subRegion.Dispose();

        //    return true;
        //}

        //public unsafe bool LEDProcess(Bitmap image, WorkParams workParam, int index)
        //{
        //    Mat search = new Mat(); //OpenCvSharp.Extensions.BitmapConverter.ToMat(image);
        //    Mat template = new Mat(); // OpenCvSharp.Extensions.BitmapConverter.ToMat(workParam.LEDAlignMarkImage);
        //    Mat result = new Mat();
        //    Mat searchPy1 = new Mat();
        //    Mat searchPy2 = new Mat();
        //    Mat searchPy3 = new Mat();
        //    Mat templatePy1 = new Mat();
        //    Mat templatePy2 = new Mat();
        //    Mat templatePy3 = new Mat();
        //    Mat mark1 = new Mat();
        //    Mat mark2 = new Mat();
        //    Mat ledSearch = new Mat();

        //    float left = 0, right = 0, top = 0, bottom = 0;
        //    int offset = 15;
        //    double minval1 = 0.0, maxval1 = 0.0, minval2 = 0.0, maxval2 = 0.0;
        //    OpenCvSharp.Point minLoc1, minLoc2, maxLoc1, maxLoc2;
        //    OpenCvSharp.Point originLoc1 = new OpenCvSharp.Point(0, 0), originLoc2 = new OpenCvSharp.Point(0, 0);

        //    bool IsSearchPyramid = false;

        //    if(!File.Exists(workParam.LEDAlignMarkImagePath))
        //    {
        //        _log.WriteLog(LogLevel.Error, LogClass.Image.ToString(), string.Format("LED Alignment 이미지 경로가 올바르지 않습니다.:{0}", workParam.LEDAlignMarkImagePath));

        //        return false;
        //    }

        //    Bitmap LEDAlignMarkImage = (Bitmap)Image.FromFile(workParam.LEDAlignMarkImagePath);

        //    if (LEDAlignMarkImage != null)
        //    {
        //        search = OpenCvSharp.Extensions.BitmapConverter.ToMat(Utils.Clone<Bitmap>(image));
        //        template = OpenCvSharp.Extensions.BitmapConverter.ToMat(Utils.Clone<Bitmap>(LEDAlignMarkImage));

        //        /*
        //        OpenCvSharp.Rect mark1 = new OpenCvSharp.Rect(2936, 143, 527, 582);
        //        OpenCvSharp.Rect mark2 = new OpenCvSharp.Rect(1948, 1226, 267, 268);

        //        Mat temp1 = search.SubMat(mark1);
        //        Mat temp2 = search.SubMat(mark2);

        //        OpenCvSharp.Extensions.BitmapConverter.ToBitmap(temp1).Save(@"F:\Image\LED Align.bmp", ImageFormat.Bmp);
        //        OpenCvSharp.Extensions.BitmapConverter.ToBitmap(temp2).Save(@"F:\Image\LED.bmp", ImageFormat.Bmp);
        //        */

        //        // 피라미드 이미지 만들기
        //        searchPy1 = search.PyrDown();
        //        searchPy2 = searchPy1.PyrDown();
        //        searchPy3 = searchPy2.PyrDown();

        //        templatePy1 = template.PyrDown();
        //        templatePy2 = templatePy1.PyrDown();
        //        templatePy3 = templatePy2.PyrDown();

        //        IsSearchPyramid = true;

        //        // 피라미드 결과 이미지(Search, Template)
        //        //OpenCvSharp.Extensions.BitmapConverter.ToBitmap(templatePy1).Save(@"F:\Image\templatepy1.bmp", ImageFormat.Bmp);
        //        //OpenCvSharp.Extensions.BitmapConverter.ToBitmap(templatePy2).Save(@"F:\Image\templatepy2.bmp", ImageFormat.Bmp);

        //        _log.WriteLog(LogLevel.Info, LogClass.Image.ToString(), string.Format("피라미드 lv3 영상 생성 완료"));

        //        // 두 개의 Alignment Mark 찾기
        //        // Template Match Level2
        //        result = searchPy3.MatchTemplate(templatePy3, TemplateMatchModes.CCoeffNormed);
        //        Cv2.MinMaxLoc(result, out minval1, out maxval1, out minLoc1, out maxLoc1);

        //        workParam.InspectionPositions[index].Similarity = Convert.ToInt32(maxval1 * 100);

        //        if (maxval1 * 100 <= workParam.LEDAlignmentSimilarityThreshold)
        //        {
        //            _log.WriteLog(LogLevel.Error, LogClass.Image.ToString(), string.Format("레벨3 마크1의 유사도가 임계치보다 작습니다.({0}<{1})", (int)(maxval1 * 100), workParam.LEDAlignmentSimilarityThreshold));

        //            search.Dispose();
        //            template.Dispose();
        //            result.Dispose();
        //            searchPy1.Dispose();
        //            searchPy2.Dispose();
        //            searchPy3.Dispose();
        //            templatePy1.Dispose();
        //            templatePy2.Dispose();
        //            templatePy3.Dispose();

        //            return false;
        //        }
        //        else
        //            _log.WriteLog(LogLevel.Info, LogClass.Image.ToString(), string.Format("레벨3 마크1(X:{0}, Y:{1}, 유사도:{2})", maxLoc1.X, maxLoc1.Y, (int)(maxval1 * 100)));

        //        // 첫번째 매칭된 위치의 결과값을 지우고, 두 번째 위치를 찾는다.
        //        left = (maxLoc1.X - templatePy3.Width / 2f < 0) ? 0 : maxLoc1.X - templatePy3.Width / 2f;
        //        top = (maxLoc1.Y - templatePy3.Height / 2f < 0) ? 0 : maxLoc1.Y - templatePy3.Height / 2f;
        //        right = (maxLoc1.X + templatePy3.Width / 2f >= result.Width) ? result.Width : maxLoc1.X + templatePy3.Width / 2f;
        //        bottom = (maxLoc1.Y + templatePy3.Height / 2f >= result.Height) ? result.Height : maxLoc1.Y + templatePy3.Height / 2f;

        //        float* data = (float*)result.DataPointer;

        //        for (int y = Convert.ToInt32(top); y < bottom; ++y)
        //        {
        //            for (int x = Convert.ToInt32(left); x < right; ++x)
        //            {
        //                data[y * result.Step1() + x] = 0.0f;
        //            }
        //        }

        //        Cv2.MinMaxLoc(result, out minval2, out maxval2, out minLoc2, out maxLoc2);

        //        workParam.InspectionPositions[index].Similarity = Convert.ToInt32(maxval2 * 100);

        //        // Up Scaling and Matching
        //        if (maxval2 * 100 <= workParam.LEDAlignmentSimilarityThreshold)
        //        {
        //            _log.WriteLog(LogLevel.Error, LogClass.Image.ToString(), string.Format("레벨3 마크2의 유사도가 임계치보다 작습니다.({0}<{1})", (int)(maxval2 * 100), workParam.LEDAlignmentSimilarityThreshold));

        //            search.Dispose();
        //            template.Dispose();
        //            result.Dispose();
        //            searchPy1.Dispose();
        //            searchPy2.Dispose();
        //            searchPy3.Dispose();
        //            templatePy1.Dispose();
        //            templatePy2.Dispose();
        //            templatePy3.Dispose();

        //            return false;
        //        }
        //        else
        //            _log.WriteLog(LogLevel.Info, LogClass.Image.ToString(), string.Format("레벨3 마크2 (X:{0}, Y:{1}, 유사도:{2})", maxLoc2.X, maxLoc2.Y, (int)(maxval2 * 100)));

        //        // 마크1, 2의 위치를 Level 0의 좌표로 변환
        //        originLoc1 = new OpenCvSharp.Point(maxLoc1.X * 8, maxLoc1.Y * 8);
        //        originLoc2 = new OpenCvSharp.Point(maxLoc2.X * 8, maxLoc2.Y * 8);

        //        // 마크1 주변 17x17 탐색하여, 최종 결과 위치 탐색
        //        left = (originLoc1.X - offset < 0) ? 0 : originLoc1.X - offset;
        //        top = (originLoc1.Y - offset < 0) ? 0 : originLoc1.Y - offset;
        //        right = (originLoc1.X + template.Width + offset + 1 >= search.Width) ? search.Width - 1 : (originLoc1.X + template.Width + offset + 1);
        //        bottom = (originLoc1.Y + template.Height + offset + 1 >= search.Height) ? search.Height - 1 : (originLoc1.Y + template.Height + offset + 1);

        //        mark1 = search.SubMat(Convert.ToInt32(top), Convert.ToInt32(bottom), Convert.ToInt32(left), Convert.ToInt32(right));

        //        result = mark1.MatchTemplate(template, TemplateMatchModes.CCoeffNormed);
        //        result.MinMaxLoc(out minval1, out maxval1, out minLoc1, out maxLoc1);

        //        workParam.InspectionPositions[index].Similarity = Convert.ToInt32(maxval1 * 100);

        //        if (maxval1 * 100 <= workParam.LEDAlignmentSimilarityThreshold)
        //        {
        //            _log.WriteLog(LogLevel.Error, LogClass.Image.ToString(), string.Format("레벨0 마크1의 유사도가 임계치보다 작습니다.({0}<{1})", (int)(maxval1 * 100), workParam.LEDAlignmentSimilarityThreshold));

        //            search.Dispose();
        //            template.Dispose();
        //            result.Dispose();
        //            searchPy1.Dispose();
        //            searchPy2.Dispose();
        //            searchPy3.Dispose();
        //            templatePy1.Dispose();
        //            templatePy2.Dispose();
        //            templatePy3.Dispose();
        //            mark1.Dispose();

        //            return false;
        //        }
        //        else
        //        {
        //            OpenCvSharp.Point realLoc1 = new OpenCvSharp.Point(left + maxLoc1.X, top + maxLoc1.Y);

        //            workParam.Blobs.Add(
        //                new Blob(
        //                    0,
        //                    realLoc1.X,
        //                    realLoc1.Y,
        //                    template.Width,
        //                    template.Height,
        //                    0,
        //                    (realLoc1.X + template.Width / 2),
        //                    (realLoc1.Y + template.Height / 2)));

        //            // 마크 1위치 저장
        //            workParam.LEDAlignMark1 = new PointF((realLoc1.X + template.Width / 2f), (realLoc1.Y + template.Height / 2f));

        //            _log.WriteLog(LogLevel.Info, LogClass.Image.ToString(), string.Format("레벨0 마크1 (X:{0}, Y:{1}, 유사도:{2})", workParam.LEDAlignMark1.X, workParam.LEDAlignMark1.Y, (int)(maxval1 * 100)));
        //        }


        //        // 마크2 주변 17x17 탐색하여, 최종 결과 위치 탐색
        //        left = (originLoc2.X - offset < 0) ? 0 : originLoc2.X - offset;
        //        top = (originLoc2.Y - offset < 0) ? 0 : originLoc2.Y - offset;
        //        right = (originLoc2.X + template.Width + offset + 1 >= search.Width) ? search.Width - 1 : (originLoc2.X + template.Width + offset + 1);
        //        bottom = (originLoc2.Y + template.Height + offset + 1 >= search.Height) ? search.Height - 1 : (originLoc2.Y + template.Height + offset + 1);

        //        mark2 = search.SubMat(Convert.ToInt32(top), Convert.ToInt32(bottom), Convert.ToInt32(left), Convert.ToInt32(right));

        //        result = mark2.MatchTemplate(template, TemplateMatchModes.CCoeffNormed);
        //        result.MinMaxLoc(out minval2, out maxval2, out minLoc2, out maxLoc2);

        //        workParam.InspectionPositions[index].Similarity = Convert.ToInt32(maxval2 * 100);

        //        if (maxval2 * 100 <= workParam.LEDAlignmentSimilarityThreshold)
        //        {
        //            _log.WriteLog(LogLevel.Error, LogClass.Image.ToString(), string.Format("레벨0 마크2의 유사도가 임계치보다 작습니다.({0}<{1})", (int)(maxval1 * 100), workParam.LEDAlignmentSimilarityThreshold));

        //            search.Dispose();
        //            template.Dispose();
        //            result.Dispose();
        //            searchPy1.Dispose();
        //            searchPy2.Dispose();
        //            searchPy3.Dispose();
        //            templatePy1.Dispose();
        //            templatePy2.Dispose();
        //            templatePy3.Dispose();
        //            mark1.Dispose();
        //            mark2.Dispose();

        //            return false;
        //        }
        //        else
        //        {
        //            OpenCvSharp.Point realLoc1 = new OpenCvSharp.Point(left + maxLoc2.X, top + maxLoc2.Y);

        //            workParam.Blobs.Add(
        //                new Blob(
        //                    1,
        //                    realLoc1.X,
        //                    realLoc1.Y,
        //                    template.Width,
        //                    template.Height,
        //                    0,
        //                    (realLoc1.X + template.Width / 2f),
        //                    (realLoc1.Y + template.Height / 2f)));

        //            // 마트2 위치와 마크 1, 2의 중심점 저장
        //            workParam.LEDAlignMark2 = new PointF((realLoc1.X + template.Width / 2f), (realLoc1.Y + template.Height / 2f));
        //            workParam.LEDAlignMarksCenter = new PointF((workParam.LEDAlignMark1.X + workParam.LEDAlignMark2.X) / 2f,
        //                (workParam.LEDAlignMark1.Y + workParam.LEDAlignMark2.Y) / 2f);

        //            _log.WriteLog(LogLevel.Info, LogClass.Image.ToString(), string.Format("레벨0 마크2 (X:{0}, Y:{1}, 유사도:{2})", workParam.LEDAlignMark2.X, workParam.LEDAlignMark2.Y, (int)(maxval2 * 100)));
        //        }
        //    }


        //    ///////////////////////////////////////////////////////////////////////////////////
        //    /// LED 위치 찾기

        //    if(!File.Exists(workParam.LEDMarkImagePath))
        //    {
        //        _log.WriteLog(LogLevel.Error, LogClass.Image.ToString(), string.Format("LED Mark 이미지 경로가 올바르지 않습니다.:{0}", workParam.LEDMarkImagePath));
        //        return false;
        //    }

        //    Bitmap LEDTemplateImage = (Bitmap)Image.FromFile(workParam.LEDMarkImagePath);

        //    if (LEDTemplateImage != null)
        //    {
        //        if (!IsSearchPyramid)
        //        {
        //            search = OpenCvSharp.Extensions.BitmapConverter.ToMat(Utils.Clone<Bitmap>(image));

        //            // 피라미드 이미지 만들기
        //            searchPy1 = search.PyrDown();
        //            searchPy2 = searchPy1.PyrDown();
        //            searchPy3 = searchPy2.PyrDown();
        //        }

        //        // LED Mark 이미지 
        //        template = OpenCvSharp.Extensions.BitmapConverter.ToMat(LEDTemplateImage);

        //        // 피라미드 수행
        //        templatePy1 = template.PyrDown();
        //        templatePy2 = templatePy1.PyrDown();
        //        templatePy3 = templatePy2.PyrDown();

        //        // 3레벨에서 탐색 영역 설정
        //        // 마크1, 2로 구성된 사각형 설정
        //        originLoc1 = new OpenCvSharp.Point(workParam.LEDAlignMark1.X, workParam.LEDAlignMark1.Y);
        //        originLoc2 = new OpenCvSharp.Point(workParam.LEDAlignMark2.X, workParam.LEDAlignMark2.Y);

        //        left = (originLoc1.X > originLoc2.X) ? originLoc2.X / 8f : originLoc1.X / 8f;
        //        right = (originLoc1.X > originLoc2.X) ? originLoc1.X / 8f : originLoc2.X / 8f;
        //        top = (originLoc1.Y > originLoc2.Y) ? originLoc2.Y / 8f : originLoc1.Y / 8f;
        //        bottom = (originLoc1.Y > originLoc2.Y) ? originLoc1.Y / 8f : originLoc2.Y / 8f;

        //        // ROI 자르기
        //        OpenCvSharp.Rect rect = new Rect(Convert.ToInt32(left), Convert.ToInt32(top), Convert.ToInt32(Math.Abs(right - left)), Convert.ToInt32(Math.Abs(bottom - top)));
        //        ledSearch = searchPy3.SubMat(rect);

        //        // 피라미드 검증용 영상
        //        //OpenCvSharp.Extensions.BitmapConverter.ToBitmap(ledSearch).Save(@"F:\Image\ledSearch.bmp", ImageFormat.Bmp);
        //        //OpenCvSharp.Extensions.BitmapConverter.ToBitmap(templatePy1).Save(@"F:\Image\LedtemplatePy1.bmp", ImageFormat.Bmp);
        //        //OpenCvSharp.Extensions.BitmapConverter.ToBitmap(templatePy2).Save(@"F:\Image\LedtemplatePy2.bmp", ImageFormat.Bmp);

        //        result = ledSearch.MatchTemplate(templatePy3, TemplateMatchModes.CCoeffNormed);
        //        result.MinMaxLoc(out minval1, out maxval1, out minLoc1, out maxLoc1);

        //        workParam.InspectionPositions[index].Similarity = Convert.ToInt32(maxval1 * 100);

        //        if (maxval1 * 100 <= workParam.LEDMarkSimilarityThreshold)
        //        {
        //            _log.WriteLog(LogLevel.Error, LogClass.Image.ToString(), string.Format("레벨3의 LED 유사도가 임계치보다 작습니다.({0}<{1})", (int)(maxval1 * 100), workParam.LEDAlignmentSimilarityThreshold));

        //            search.Dispose();
        //            template.Dispose();
        //            result.Dispose();
        //            searchPy1.Dispose();
        //            searchPy2.Dispose();
        //            templatePy1.Dispose();
        //            templatePy2.Dispose();
        //            mark1.Dispose();
        //            mark2.Dispose();
        //            ledSearch.Dispose();

        //            return false;
        //        }

        //        // LED 마크의 레벨0 위치 설정
        //        originLoc1 = new OpenCvSharp.Point((left + maxLoc1.X) * 8, (top + maxLoc1.Y) * 8);

        //        // 17x17 탐색
        //        left = (originLoc1.X - offset < 0) ? 0 : originLoc1.X - offset;
        //        top = (originLoc1.Y - offset < 0) ? 0 : originLoc1.Y - offset;
        //        right = (originLoc1.X + template.Width + offset + 1 >= search.Width) ? search.Width - 1 : (originLoc1.X + template.Width + offset + 1);
        //        bottom = (originLoc1.Y + template.Height + offset + 1 >= search.Height) ? search.Height - 1 : (originLoc1.Y + template.Height + offset + 1);

        //        mark1 = search.SubMat(Convert.ToInt32(top), Convert.ToInt32(bottom), Convert.ToInt32(left), Convert.ToInt32(right));

        //        result = mark1.MatchTemplate(template, TemplateMatchModes.CCoeffNormed);
        //        result.MinMaxLoc(out minval1, out maxval1, out minLoc1, out maxLoc1);

        //        //OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mark1).Save(@"F:\Image\originLEDMark.bmp", ImageFormat.Bmp);

        //        workParam.InspectionPositions[index].Similarity = Convert.ToInt32(maxval1 * 100);

        //        if (maxval1 * 100 <= workParam.LEDMarkSimilarityThreshold)
        //        {
        //            _log.WriteLog(LogLevel.Error, LogClass.Image.ToString(), string.Format("레벨0 마크1의 유사도가 임계치보다 작습니다.({0}<{1})", (int)(maxval1 * 100), workParam.LEDAlignmentSimilarityThreshold));

        //            search.Dispose();
        //            template.Dispose();
        //            result.Dispose();
        //            searchPy1.Dispose();
        //            searchPy2.Dispose();
        //            templatePy1.Dispose();
        //            templatePy2.Dispose();
        //            mark1.Dispose();
        //            mark2.Dispose();
        //            ledSearch.Dispose();

        //            return false;
        //        }
        //        else
        //        {
        //            OpenCvSharp.Point realLoc1 = new OpenCvSharp.Point(left + maxLoc1.X, top + maxLoc1.Y);

        //            workParam.Blobs.Add(
        //                new Blob(
        //                    2,
        //                    realLoc1.X,
        //                    realLoc1.Y,
        //                    template.Width,
        //                    template.Height,
        //                    0,
        //                    (realLoc1.X + template.Width / 2),
        //                    (realLoc1.Y + template.Height / 2)));

        //            // LED 마크 위치와 얼라인마크 1, 2의 중심 사이의 거리 계산
        //            workParam.LEDMark = new PointF((realLoc1.X + template.Width / 2f), (realLoc1.Y + template.Height / 2f));

        //            workParam.InspectionPositions[index].Distance = (float)Math.Sqrt(Math.Pow(workParam.LEDAlignMarksCenter.X - workParam.LEDMark.X, 2)
        //                + Math.Pow(workParam.LEDAlignMarksCenter.Y - workParam.LEDMark.Y, 2)); ;

        //            _log.WriteLog(LogLevel.Info, LogClass.Image.ToString(), string.Format("LED (X:{0}, Y:{1}, 유사도:{2})", workParam.LEDMark.X, workParam.LEDMark.Y, (int)(maxval1 * 100)));
        //        }
        //    }

        //    return true;
        //}
    }
}
