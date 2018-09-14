﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FytMsys.Common;
using FytMsys.Helper;
using FytMsys.Logic.Admin;

namespace FytMsys.Web.Controllers
{
    /// <summary>
    /// 文件上传
    /// </summary>
    public class FileUpController : Controller
    {
        /// <summary>
        /// 单文件上传
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SignUpFile()
        {
            var jsonM = new JsonHelper.JsonAjaxModel() { Status = "y", Msg = "success" };
            try
            {
                HttpPostedFileBase upfile = Request.Files["signfile"]; //取得上传文件
                //上传类型 0=图片 1=文件 2=视频
                var isImg = 0;
                //是否缩略图  0=不压缩  1=压缩
                var isThum = 0;
                //是否添加水印
                var isWater =0;
                if (upfile == null)
                {
                    jsonM.Status = "err";
                    jsonM.Msg = "请选择要上传文件！";
                    return Json(jsonM);
                }
                var jsFile = UpLoadHelper.SavePictureWeb(upfile, 650, 400, "Cut");
                jsonM.Status = jsFile.Status;
                jsonM.Msg = jsFile.Msg;
                jsonM.Data = jsFile.ImgUrl;
            }
            catch (Exception ex)
            {
                jsonM.Status = "err";
                jsonM.Msg = "上传过程中发生错误，消息：" + ex.Message;
                LogHelper.WriteLog(typeof(FileUpController), ex);
            }
            return Json(jsonM);
        }

        /// <summary>
        /// 单文件上传(Banner)
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult BannerUpFile()
        {
            var jsonM = new JsonHelper.JsonAjaxModel() { Status = "y", Msg = "success" };
            try
            {
                HttpPostedFileBase upfile = Request.Files["bannerfile"]; //取得上传文件
                //上传类型 0=图片 1=文件 2=视频
                var isImg = 0;
                //是否缩略图  0=不压缩  1=压缩
                var isThum = 0;
                //是否添加水印
                var isWater = 0;
                if (upfile == null)
                {
                    jsonM.Status = "err";
                    jsonM.Msg = "请选择要上传文件！";
                    return Json(jsonM);
                }
                var jsFile = UpLoadHelper.SavePictureWeb(upfile, 1920, 500, "Cut");
                jsonM.Status = jsFile.Status;
                jsonM.Msg = jsFile.Msg;
                jsonM.Data = jsFile.ImgUrl;
            }
            catch (Exception ex)
            {
                jsonM.Status = "err";
                jsonM.Msg = "上传过程中发生错误，消息：" + ex.Message;
                LogHelper.WriteLog(typeof(FileUpController), ex);
            }
            return Json(jsonM);
        }

        /// <summary>
        /// 单文件上传(展示图)
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ShowUpFile()
        {
            var jsonM = new JsonHelper.JsonAjaxModel() { Status = "y", Msg = "success" };
            try
            {
                HttpPostedFileBase upfile = Request.Files["showfile"]; //取得上传文件
                //上传类型 0=图片 1=文件 2=视频
                var isImg = 0;
                //是否缩略图  0=不压缩  1=压缩
                var isThum = 0;
                //是否添加水印
                var isWater = 0;
                if (upfile == null)
                {
                    jsonM.Status = "err";
                    jsonM.Msg = "请选择要上传文件！";
                    return Json(jsonM);
                }
                var jsFile = UpLoadHelper.SavePictureWeb(upfile, 200, 200, "Cut");
                jsonM.Status = jsFile.Status;
                jsonM.Msg = jsFile.Msg;
                jsonM.Data = jsFile.ImgUrl;
            }
            catch (Exception ex)
            {
                jsonM.Status = "err";
                jsonM.Msg = "上传过程中发生错误，消息：" + ex.Message;
                LogHelper.WriteLog(typeof(FileUpController), ex);
            }
            return Json(jsonM);
        }

        /// <summary>
        /// 前台多文件上传
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult WebUpload()
        {
            var jsonM = new JsonHelper.JsonAjaxModel() { Status = "y", Msg = "success" };
            var hfc = Request.Files;
            if (hfc.Count > 0)
            {
                for (int i = 0; i < Request.Files.Count; i++)
                {
                    string smallPic;
                    HttpPostedFileBase hpf = Request.Files[i];
                    var jsFile = UpLoadHelper.SavePictureWeb(hpf, 700, 400, "W");
                    jsonM.Status = jsFile.Status;
                    jsonM.Msg = jsFile.Msg;
                    jsonM.Data = jsFile.ImgUrl;
                }
            }
            return Json(jsonM);
        }

    }
}