using OpenCvSharp;
using OpenVinoSharp.Extensions.model.PaddleOCR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OcrConsole4._8
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            OnlineOcr ocr = await Pipeline.GetOnlineOCR(Language.ch_PP_OCRv4);
            Tuple<List<OCRPredictResult>, Mat> ocr_result = ocr.ocr_test();
            PaddleOcrUtility.print_result(ocr_result.Item1);
            Mat image = PaddleOcrUtility.visualize_bboxes(ocr_result.Item2, ocr_result.Item1);
            Cv2.ImShow("Result", image);
            Cv2.WaitKey(0);
        }
    }
}
