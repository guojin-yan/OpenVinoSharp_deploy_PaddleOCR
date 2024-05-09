![OpenVINO™ C# API](https://socialify.git.ci/guojin-yan/OpenVINO-CSharp-API/image?description=1&descriptionEditable=💞%20OpenVINO%20wrapper%20for%20.NET💞%20&forks=1&issues=1&logo=https%3A%2F%2Fs2.loli.net%2F2023%2F01%2F26%2FylE1K5JPogMqGSW.png&name=1&owner=1&pattern=Circuit%20Board&pulls=1&stargazers=1&theme=Light)

<p align="center">    
    <a href="./LICENSE.txt">
        <img src="https://img.shields.io/github/license/guojin-yan/openvinosharp.svg">
    </a>    
    <a >
        <img src="https://img.shields.io/badge/Framework-.NET 6.0%2C%20.NET 5.0%2C%20.NET Framework 4.8%2C%20.NET Framework 4.7.2%2C%20.NET Core 3.1-pink.svg">
    </a>    
</p>

# 使用 OpenVINO<sup>TM </sup> C# API 部署 PaddleOCR

该项目主要基于开发的[OpenVINO<sup>TM </sup> C# API](OpenVINO<sup>TM </sup> C# API)项目，基于 C# 编程语言在.NET框架下使用[OpenVINO<sup>TM </sup>](https://github.com/openvinotoolkit/openvino)部署工具部署百度飞桨下的 [PaddleOCR](https://github.com/PaddlePaddle/PaddleOCR) 系列模型，实现图片文字识别、版面分析以及表格识别等功能。

项目提供了简单的案例以及二次开发的API接口，大家可以根据自己需求进行再次开发与使用。为了方便开发者使用，该项目以及发布了NuGet Package程序集:

| Package                                      | Description                              | Link                                                         |
| -------------------------------------------- | ---------------------------------------- | ------------------------------------------------------------ |
| **OpenVINO.CSharp.API.Extensions.PaddleOCR** | PaddleOCR Deploy in OpenVINO.CSharp.API. | [![NuGet Gallery ](https://badge.fury.io/nu/OpenVINO.CSharp.API.Extensions.PaddleOCR.svg)](https://www.nuget.org/packages/OpenVINO.CSharp.API.Extensions.PaddleOCR) |

# 🛠 项目环境

在本项目中主要使用的是自己开发的**OpenVINO<sup>TM </sup> C# API**项目以及**OpenCvSharp4**项目，所使用**NuGet Package**程序包以及安装方式如下所示:

## <img title="NuGet" src="https://s2.loli.net/2023/08/08/jE6BHu59L4WXQFg.png" alt="" width="40"> NuGet Package

- **OpenVINO.CSharp.API.Extensions.PaddleOCR >= 1.01**


- **OpenVINO.runtime.win >= 2023.2.0.1**
- **OpenCvSharp4.runtime.win >= 4.9.0.20240103**

## 😇 安装方式

下面根据该程序集的使用情况，介绍一下一般情况下该程序集的使用方法：

&emsp;    首先是安装必要的程序集，主要包括本文的OpenVINO™ .CSharp.API.Extensions.PaddleOCR程序集，同时还包括图像处理库OpenCvSharp程序集，通过``dotnet add package``安装方式安装输入以下指令即可：

```shell
dotnet add package OpenCvSharp4
dotnet add package OpenVINO.CSharp.API.Extensions.PaddleOCR
```

&emsp;    同时，我们还需要安装必要的Runtime库，主要是OpenCv和OpenVINO，通过``dotnet add package``安装方式安装输入以下指令即可：

```shell
dotnet add package OpenCvSharp4.runtime.win
dotnet add package OpenVINO.runtime.win
```

&emsp;    此外，如果使用Visual Studio 的NuGet Package安装，只需要安装以上是个程序集即可。

# 🎯应用案例演示

##  ``OnlineOcr``：在线模型识别

&emsp;    ``OnlineOcr``是封装的一个简单的在线模型识别方法，可以下载官方的PP-OCR文本识别模型到本地，然后使用OpenVINO加载模型创建推理器，此处为了方便测试，提供了一个``ocr_test()``接口，可以下载在线图片进行检测，检验项目是否安装成功。

&emsp;    下面是一个十分简单的测试代码，通过``Pipeline.GetOnlineOCR()``获取在线模型，然后调用``ocr_test()``进行预测，如下所示：

```csharp
using OpenCvSharp;
using OpenVinoSharp.Extensions.model.PaddleOCR;

namespace OcrConsole
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
```

&emsp;    下图为上述程序运行后输出，首先会下载相应的模型文件以及标签文件，在推理时会下载测试图片，然后完成推理，实现文本检测：

![image-20240430163802704](https://s2.loli.net/2024/04/30/18IVJrmwvsTOKji.png)

&emsp;    为了方便开发者使用，此处同时也提供了其他模型的下载方式，目前已经包含了PP-OCR所有模型，可以通过设置``Language``来实现不同语言模型的下载，同时还可以使用``OCRDetModels.Get()``、``OCRRecModels.Get()``、``OCRClsModels.Get()``等接口下载其他的模型。

## ``OCRPredictor``：本地模型预测

&emsp;    ``OCRPredictor``是该程序集中主要封装的预测模块，该模块可以实现本地模型的加载，并调用模型进行OCR识别，同时支持单一过程识别，开发者可以根据自己的需求，设置或者使用单一或其中几个过程进行推理，进行自己的项目开发和配置。

&emsp;    下面代码演示了两种``OCRPredictor``初始化方式：

- 第一种使用模型文件路径进行初始化，此处需要提前设置``RuntimeOption.RecOption.label_path``识别模型key文件路径，``RuntimeOption``是该程序集中封装的常营配置参数，用户可以根据自己的使用需求进行修改；然后设置指定的模型路径初始化``OCRPredictor``即可；初始化完成后，调用``ocr()``方法进行图片预测。

```csharp
using OpenCvSharp;
using OpenVinoSharp.Extensions.model.PaddleOCR;
namespace OcrConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string det_model = "./ch_PP-OCRv4_det_infer/inference.pdmodel";
            string cls_model = "./ch_ppocr_mobile_v2.0_cls_infer/inference.pdmodel";
            string rec_model = "./ch_PP-OCRv4_rec_infer/inference.pdmodel";
            string key_path = "ppocr_keys_v1.txt";
            
            RuntimeOption.RecOption.label_path = key_path;
            OCRPredictor ocr = new OCRPredictor(det_model, cls_model, rec_model);
            
            string img_path = "demo_1.jpg";
            Mat img = Cv2.ImRead(img_path);
            
            List<OCRPredictResult> ocr_result = ocr.ocr(img, true, true, true);
            PaddleOcrUtility.print_result(ocr_result);
            Mat new_image = PaddleOcrUtility.visualize_bboxes(img, ocr_result);
            Cv2.ImShow("result", new_image);
            Cv2.WaitKey(0);
        }
    }
}
```

- 第二种使用``OcrConfig``进行初始化，``OcrConfig``是该程序集中定义的配置类，里面定义了OCR预测时常用的一些配置参数，包括模型路径、key文件路径等，初始化``OcrConfig``后，便可以设置预测模型路径以及其它参数，最后将初始化的``OcrConfig``带入到``OCRPredictor``即可；初始化完成后，调用``ocr()``方法进行图片预测。

```csharp
using OpenCvSharp;
using OpenVinoSharp.Extensions.model.PaddleOCR;
namespace OcrConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string det_model = "./ch_PP-OCRv4_det_infer/inference.pdmodel";
            string cls_model = "./ch_ppocr_mobile_v2.0_cls_infer/inference.pdmodel";
            string rec_model = "./ch_PP-OCRv4_rec_infer/inference.pdmodel";
            string key_path = "ppocr_keys_v1.txt";
            
            OcrConfig config = new OcrConfig();
            config.set_det_model_path(det_model);
            config.set_cls_model_path(cls_model);
            config.set_rec_model_path(rec_model);
            config.set_rec_dict_path(key_path);
            OCRPredictor ocr = new OCRPredictor(config);
            
            string img_path = "demo_1.jpg";
            Mat img = Cv2.ImRead(img_path);
            
            List<OCRPredictResult> ocr_result = ocr.ocr(img, true, true, true);
            PaddleOcrUtility.print_result(ocr_result);
            Mat new_image = PaddleOcrUtility.visualize_bboxes(img, ocr_result);
            Cv2.ImShow("result", new_image);
            Cv2.WaitKey(0);
        }
    }
}
```

&emsp;    下图为使用本地模型以及图片预测后的输出情况：

![image-20240430193805438](https://s2.loli.net/2024/04/30/HQcgtwIWxirBqnf.png)

&emsp;    在使用``ocr()``方法预测时，从我们可以通过指定标志位来定义本次预测流程，如下所示：

```csharp
List<OCRPredictResult> ocr_result = ocr.ocr(img, det: true, rec: true, cls: true);
```

&emsp;    其中后面标志位``det``、``rec``、``cls``分别指示本次推理是否开启该流程，如果本次不需要文本区域识别，只进行文字方向判断以及文本识别，则需要设置为：

```csharp
List<OCRPredictResult> ocr_result = ocr.ocr(img, det: false, rec: true, cls: true);
```

&emsp;    当然，``OCRPredictor``也同时提供了单一推理方式的接口，使用方式为：

```csharp
List<OCRPredictResult> result = ocr.det(img);
List<Mat> img_list = new List<Mat>();
for (int j = 0; j < result.Count; j++)
{
    Mat crop_img = new Mat();
    crop_img = PaddleOcrUtility.get_rotate_crop_image(img, result[j].box);
    img_list.Add(crop_img);
}
result = ocr.cls(img_list, result);
result = ocr.rec(img_list, result);
```

&emsp;    以上就是对OpenVINO™ .CSharp.API.Extensions.PaddleOCR NuGet Package简单的使用介绍，更多的实用信息可以访问源码进行查看。

# 📱总结

&emsp;    在该项目中，我们结合开发的OpenVINO™.CSharp.API.Extensions.PaddleOCR NuGet Package向大家展示了其简单的使用方法，方便大家快速上手该项目，并结合自己的应用需求进行DIY开发。最后如果各位开发者在使用中有任何问题，以及对该接口开发有任何建议，欢迎大家与我联系。

<div align=center><img src="https://s2.loli.net/2024/01/29/VIPU1MSwjEh2QAY.png" width=800></div>

