using OpenCvSharp;
using OpenVinoSharp.Extensions.model.PaddleOCR;

namespace test_online_model
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            //OcrModel ocrModel = await OcrModel.GetOnlineOcrModel(Language.ch_PP_OCRv3,false,false,true);
            OnlineOcr ocr = await Pipeline.GetOnlineOCR(Language.ch_PP_OCRv4);
            List<OCRPredictResult> ocr_result = ocr.predict();
            PaddleOcrUtility.print_result(ocr_result);
            //Mat new_image = PaddleOcrUtility.visualize_bboxes(image, ocr_result);
        }
    }
}
