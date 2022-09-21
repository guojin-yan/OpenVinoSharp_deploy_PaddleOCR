﻿using System;
using OpenCvSharp;

namespace OpenVinoSharpPaddleOCR
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // 测试图片路径
            string image_path = @"E:\Git_space\基于Csharp和OpenVINO部署PaddleOCR模型\image\demo_1.jpg";
            Mat image = Cv2.ImRead(image_path);
            Cv2.ImShow("image", image);


            //------------------------------一、文字区域识别-----------------------//
            Console.WriteLine("//------------------------------一、文字区域识别-----------------------//");

            //*******************1.加载模型相关信息****************//
            // 模型相关参数
            // 模型路径
            string model_file_path_det = @"D:\model\det_server_onnx\model.onnx";
            // 设备名
            string device_name = "CPU";

            // 模型输入节点
            string input_node_name_det = "x";
            // 模型输出节点
            string output_node_name_det = "save_infer_model/scale_0.tmp_1";

            //*******************2.初始化推理核心****************//
            Core pridector_det = new Core(model_file_path_det, device_name);

            //*******************3.配置模型推理输入数据****************//

            // 设置模型节点形状
            ulong[] input_size_det = new ulong[] { 1, 3, 640, 640 };
            pridector_det.set_input_sharp(input_node_name_det, input_size_det);
            // 设置输入数据
            byte[] image_data_det = new byte[2048 * 2048 * 3];
            ulong image_size_det = new ulong();
            image_data_det = image.ImEncode(".bmp");
            image_size_det = Convert.ToUInt64(image_data_det.Length);
            // 将图片数据加载到模型
            pridector_det.load_input_data(input_node_name_det, image_data_det, image_size_det, 0);

            //*******************4.模型推理****************//
            // 模型推理
            pridector_det.infer();
            //*******************5.读取模型输出数据****************//
            int result_det_length = 640 * 640;
            float[] result_det = pridector_det.read_infer_result<float>(output_node_name_det, result_det_length);

            //*******************6.处理模型推理数据****************//
            // 将模型输出转为byte格式
            byte[] result_det_byte = new byte[result_det_length];
            for (int i = 0; i < result_det_length; i++)
            {
                result_det_byte[i] = (byte)(result_det[i] * 255);
            }
            // 重构结果图像
            Mat image_det = new Mat(640, 640, MatType.CV_8UC1, result_det_byte);
            Cv2.ImShow("image_det", image_det);
            // 查找文字区域狂
            Rect[] text_rects = find_rect(image_det);

            Mat image_rect = image.Clone();
            Cv2.Resize(image_rect, image_rect, new Size(640, 640));
            //裁剪文字区域
            Mat[] text_images = cut_image_roi(image_rect, text_rects);
            // 将文字区域标注出来

            for (int r = 0; r < text_rects.Length; r++)
            {
                Cv2.Rectangle(image_rect, text_rects[r], new Scalar(0, 0, 255), 2);
            }
            Cv2.ImShow("image_rect", image_rect);


            //------------------------------二、文字内容识别-----------------------//
            Console.WriteLine("//------------------------------二、文字内容识别-----------------------//");
            //*******************1.加载模型相关信息****************//
            // 模型相关参数
            // 模型路径
            string model_file_path_rec = @"D:\model\rec_server_onnx\model.onnx";

            // 模型输入节点
            string input_node_name_rec = "x";
            // 模型输出节点
            string output_node_name_rec = "save_infer_model/scale_0.tmp_1";
            // 字符字典
            string dict_path = @"E:\Git_space\基于Csharp和OpenVINO部署PaddleOCR模型\model\ppocr_keys_v1.txt";


            //*******************2.初始化推理核心****************//
            Core pridector_rec = new Core(model_file_path_rec, device_name);
            // 读取字符查询字典
            List<string> dicts = read_dict(dict_path);

            //*******************3.逐张推理识别文字内容****************//
            for (int epoch = 0; epoch < text_images.Length; epoch++)
            {

                //*****************3.1 调整推理图片形状**************//
                Mat text_image = text_images[epoch].Clone();
                text_image = adjust_image_size(text_image);

                //*****************3.2 设置模型输入节点形状**************//
                ulong[] input_size_rec = new ulong[] { 1, 3, (ulong)text_image.Height, (ulong)text_image.Width };
                pridector_rec.set_input_sharp(input_node_name_rec, input_size_rec);

                //*****************3.3 加载模型推数据**************//
                // 设置输入数据
                byte[] image_data_rec = new byte[2048 * 2048 * 3];
                ulong image_size_rec = new ulong();
                image_data_rec = text_image.ImEncode(".bmp");
                image_size_rec = Convert.ToUInt64(image_data_rec.Length);
                // 将图片数据加载到模型
                pridector_det.load_input_data(input_node_name_rec, image_data_rec, image_size_rec, 0);

                //*******************3.4.模型推理****************//
                // 模型推理
                pridector_rec.infer();
                //*******************5.读取模型输出数据****************//
                int text_size = text_image.Width / 4;
                int result_rec_length = text_size * 6625;
                float[] result_rec = pridector_rec.read_infer_result<float>(output_node_name_rec, result_rec_length);

                string text = process_rec_result(result_rec, text_size, dicts);
                Console.WriteLine(text);

            }

            Console.WriteLine(12);
            Cv2.WaitKey(0);
        }


        public static Rect[] find_rect(Mat source_image)
        {
            Mat image = source_image.Clone();
            //中值滤波或腐蚀去除噪点
            Cv2.MedianBlur(image, image, 3);
            Mat element = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(1, 1), new Point(-1, -1));
            Cv2.Erode(image, image, element, new Point(-1, -1), 1, BorderTypes.Default, new Scalar());
            //Cv2.ImShow("erode", diff);
            Mat element2 = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(2, 2), new Point(-1, -1));
            Cv2.Dilate(image, image, element2, new Point(-1, -1), 1, BorderTypes.Default, new Scalar());
            //Cv2.ImShow("dilate", diff);

            Point[][] contours;
            HierarchyIndex[] hierarchy; //轮廓拓扑结构变量
            Cv2.FindContours(image, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxNone);

            Rect[] rects = new Rect[contours.Length];
            for (int i = 0; i < contours.Length; i++)
            {
                Rect rect = Cv2.BoundingRect(contours[i]);
                rect = enlarge_rect(rect);
                Console.WriteLine(rect);
                rects[i] = rect;
            }
            return rects;
        }

        public static Rect enlarge_rect(Rect rect)
        {
            Rect rect_temp = new Rect();
            Point point = new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
            int width = 0;
            int height = 0;
            // 判断矩形区域横纵向
            if (rect.Width > rect.Height)
            {
                if (rect.Width < 80)
                {
                    width = (int)((double)rect.Width * 1.5);
                }
                else
                {
                    width = (int)((double)rect.Width * 1.1);
                }
                height = (int)((double)rect.Height * 3);
            }
            else
            {
                if (rect.Height < 80)
                {
                    height = (int)((double)rect.Height * 1.5);
                }
                else
                {
                    height = (int)((double)rect.Height * 1.1);
                }
                width = (int)((double)rect.Width * 2.5);

            }
            // 判断矩形框是否超边界
            if (point.X - width / 2 < 0)
            {
                width = width + (point.X - width / 2) * 2;
            }
            if (point.X + width / 2 > 640)
            {
                width = width - (640 - point.X - width / 2) * 2;
            }
            if (point.Y - height / 2 < 0)
            {
                height = height + (point.Y - height / 2) * 2;
            }
            if (point.Y + height / 2 > 640)
            {
                height = height - (640 - point.Y - height / 2) * 2;
            }

            rect_temp = new Rect(point.X - width / 2 + 1, point.Y - height / 2 - 1, width - 2, height - 2);

            return rect_temp;
        }

        public static Mat[] cut_image_roi(Mat source_image, Rect[] rects)
        {
            Mat[] rois = new Mat[rects.Length];

            for (int r = 0; r < rects.Length; r++)
            {
                Mat roi = new Mat(source_image, rects[r]);
                rois[r] = roi;
            }
            return rois;

        }

        public static List<string> read_dict(string dict_path)
        {
            List<string> list = new List<string>();
            StreamReader str = new StreamReader(dict_path);
            while (true)
            {
                string line = str.ReadLine();
                if (line == null)
                {
                    break;
                }
                list.Add(line);
            }
            return list;
        }

        public static Mat adjust_image_size(Mat source_image)
        {
            int img_W = source_image.Width;
            int img_H = 32;
            double scale_size = (double)img_W / (double)source_image.Height;

            int max_W = (int)scale_size * 32;
            if (scale_size * img_H > max_W)
            {
                img_W = max_W;
            }
            else
            {
                img_W = (int)scale_size * img_H;
            }
            Cv2.Resize(source_image, source_image, new Size(img_W, img_H));
            return source_image;
        }

        public static string process_rec_result(float[] result, int text_size, List<string> dict)
        {
            float[] confindences = new float[text_size];
            int[] indexs = new int[text_size];
            for (int r = 0; r < text_size; r++)
            {
                float[] temp = new float[6625];
                for (int j = 0; j < 6625; j++)
                {
                    temp[j] = result[r + 6625 + j];
                }
                int index = 0;
                float max = max_index(temp, ref index);
                indexs[r] = index;
                confindences[r] = max;
            }
            float aver_confindence = confindences.Average();
            List<string> list = new List<string>();
            for (int r = 0; r < text_size; r++)
            {
                if (text_size < 20 && confindences[r] > 0)
                {
                    list.Add(dict[indexs[r] - 1]);
                }
                else 
                {
                    if (confindences[r] > aver_confindence - 0.5) 
                    {
                        list.Add(dict[indexs[r] - 1]);
                    }
                }
            }
            return String.Join("", list.ToArray());
        }
        public static float max_index(float[] data, ref int index) 
        {
            float temp = data[0];
            for (int i = 0; i < data.Length; i++) 
            {
                if (temp > data[i]) { 
                    index = i;
                    temp = data[i];
                }
            }
            return temp;
        }
    }
}