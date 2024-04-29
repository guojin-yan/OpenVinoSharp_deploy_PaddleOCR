using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenCvSharp;
using OpenVinoSharp.Extensions.model.PaddleOCR;
namespace OcrConsoleNet5._0
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            OnlineOcr ocr = await Pipeline.GetOnlineOCR(Language.ch_PP_OCRv4);
            Tuple<List<OCRPredictResult>, Mat> ocr_result = ocr.ocr_test();
            PaddleOcrUtility.print_result(ocr_result.Item1);
        }
    }
}
