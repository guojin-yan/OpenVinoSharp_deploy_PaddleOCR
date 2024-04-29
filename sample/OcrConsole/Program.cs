using OpenCvSharp;
using OpenVinoSharp;
using OpenVinoSharp.Extensions.model.PaddleOCR;
namespace OcrConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {

            OcrConfig config = new OcrConfig();

            string image_path = "";
            if (args.Length == 1)
            {
                string base_path = Path.GetFullPath(args[0]);
                if (!PaddleOcrUtility.chech_path(base_path))
                {
                    return;
                }
                config.det_model_path = Path.Combine(base_path, "model/paddle/ch_PP-OCRv4_det_infer/inference.pdmodel");
                config.cls_model_path = Path.Combine(base_path, "model/paddle/ch_ppocr_mobile_v2.0_cls_infer/inference.pdmodel");
                config.rec_model_path = Path.Combine(base_path, "model/paddle/ch_PP-OCRv4_rec_infer/inference.pdmodel");
                image_path = Path.Combine(base_path + "/image/demo_1.jpg");
                config.set_rec_dict_path(Path.Combine(base_path + "dict/ppocr_keys_v1.txt"));
            }
            else if (args.Length == 5)
            {
                image_path = args[0];
                config.det_model_path = args[1];
                config.cls_model_path = args[2];
                config.rec_model_path = args[3];
                config.set_rec_dict_path(args[4]);
            }
            else
            {
                Console.WriteLine("Please add command function parameters, such as:");
                Console.WriteLine(" >dotnet run <dir_path>");
                Console.WriteLine(" >dotnet run <image_path> <det_model_path> <cls_model_path> <rec_model_path> <dict_path_path>");
                return;
            }


            //Core core = new Core();
            //string s = core.get_property("CPU",PropertyKey.SUPPORTED_PROPERTIES);
            //Console.WriteLine(s);
            //Model model = core.read_model(config.rec_model_path);
            //CompiledModel compiled_model = core.compile_model(model, "CPU");

            //s = compiled_model.get_property("SUPPORTED_PROPERTIES");

            //Console.WriteLine(s);



            if (PaddleOcrUtility.chech_file(config.det_model_path) || PaddleOcrUtility.chech_file(config.cls_model_path) || PaddleOcrUtility.chech_file(config.rec_model_path)) { }
            else { return; }

            if (!PaddleOcrUtility.chech_file(config.rec_option.label_path)) { return; }

            if (!PaddleOcrUtility.chech_file(image_path))
            {
                return;
            }

            config.cls_option.batch_num = 1;
            config.rec_option.batch_num = 1;
            //config.rec_option.device = "GPU.0";
            //config.rec_option.use_gpu = true;
            OCRPredictor ocr = new OCRPredictor(config);

            Mat image = Cv2.ImRead(image_path);
            List<OCRPredictResult> ocr_result = ocr.ocr(image, true, true, true);
            Console.WriteLine("----------------- ");
            DateTime start = DateTime.Now;
            ocr_result = ocr.ocr(image, true, true, true);
            DateTime end = DateTime.Now;
            Console.WriteLine("time: " + (end - start).TotalMilliseconds);
            PaddleOcrUtility.print_result(ocr_result);
            Mat new_image = PaddleOcrUtility.visualize_bboxes(image, ocr_result);
            Cv2.ImShow("result", new_image);
            Cv2.WaitKey(0);
        }
    }
}
