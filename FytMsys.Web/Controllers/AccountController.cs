﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Domain.Entity;
using FytMsys.Common;
using FytMsys.Helper;
using Domain.ViewModel;
using System.Globalization;

namespace FytMsys.Web.Controllers
{
    /// <summary>
    /// 个人中心
    /// </summary>
    [AccountFilter]
    public class AccountController : Controller
    {
        /// <summary>
        /// 个人中心主页
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View(GetSession());
        }

        /// <summary>
        /// 上传认证资料
        /// </summary>
        /// <returns></returns>
        public ActionResult AuthUser()
        {
            return View(GetSession());
        }

        /// <summary>
        /// 获得登录用户模型
        /// </summary>
        /// <returns></returns>
        public tb_User GetSession()
        {
            var user = new tb_User();
            if (HttpContext.Session != null)
            {
                user = HttpContext.Session["FytUser"] as tb_User;
            }
            return user;
        }

        /// <summary>
        /// 编辑用户基本信息
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult EditUser(tb_User parModel)
        {
            var jsonM = new JsonHelper.JsonAjaxModel() { Status = "y", Msg = "用户数据修改成功！", BackUrl = "/" };
            try
            {
                var temo = GetSession();
                var user = OperateContext<tb_User>.SetServer.GetModel(m => m.ID == temo.ID);
                user.NickName = parModel.NickName;
                user.TrueName = parModel.TrueName;
                user.Sex = parModel.Sex;
                user.Province = parModel.Province;//区号
                user.Mobile = parModel.Mobile;
                user.SafeAnswer = parModel.SafeAnswer;//个人介绍
                user.SafeQuestion = parModel.SafeQuestion;//职业
                user.QQ = parModel.QQ;
                OperateContext<tb_User>.SetServer.Update(user);

                if (HttpContext.Session != null) HttpContext.Session["FytUser"] = user;
            }
            catch (Exception ex)
            {
                jsonM.Status = "err";
                jsonM.Msg = "发生错误！消息：" + ex.Message;
                LogHelper.WriteLog(typeof(AccountController), ex);
            }
            return Json(jsonM);
        }

        /// <summary>
        /// 退出账户
        /// </summary>
        /// <returns></returns>
        public ActionResult SignOut()
        {
            if (HttpContext.Session != null) HttpContext.Session["FytUser"] = null;
            UtilsHelper.WriteCookie("FytUserId", "");
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// 头像上传
        /// </summary>
        /// <returns></returns>
        public ActionResult UpHeadPic()
        {
            var jsonM = new JsonHelper.JsonAjaxModel() { Status = "y", Msg = "success" };

            string upFiles = FytRequest.GetQueryString("upFiles");
            HttpPostedFileBase pfb = Request.Files[upFiles];
            var jsFile = UpLoadHelper.SavePicture(pfb, 150, 150, "HW");
            jsonM.Status = jsFile.Status;
            jsonM.Msg = jsFile.Msg;
            jsonM.Data = jsFile.ImgUrl;
            //更新数据库
            var temo = GetSession();
            var user = OperateContext<tb_User>.SetServer.GetModel(m => m.ID == temo.ID);
            user.HeadPic = jsFile.ImgUrl;
            OperateContext<tb_User>.SetServer.Update(user);
            if (HttpContext.Session != null) HttpContext.Session["FytUser"] = user;
            return Json(jsonM);
        }

        /// <summary>
        /// 资料上传
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UpAuthDocs()
        {
            var jsonM = new JsonHelper.JsonAjaxModel() { Status = "y", Msg = "success" };

            string upFiles = FytRequest.GetQueryString("upFiles");
            HttpPostedFileBase pfb = Request.Files[upFiles];
            var jsFile = UpLoadHelper.SavePicture(pfb, 500, 300, "HW");
            jsonM.Status = jsFile.Status;
            jsonM.Msg = jsFile.Msg;
            jsonM.Data = jsFile.ImgUrl;
            //更新数据库
            var temo = GetSession();
            var user = OperateContext<tb_User>.SetServer.GetModel(m => m.ID == temo.ID);
            user.Address = jsFile.ImgUrl;
            OperateContext<tb_User>.SetServer.Update(user);
            if (HttpContext.Session != null) HttpContext.Session["FytUser"] = user;
            return Json(jsonM);
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult EditPwd()
        {
            return View();
        }

        /// <summary>
        /// 用户修改密码
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPwd()
        {
            var jsonM = new JsonHelper.JsonAjaxModel() { Status = "y", Msg = "success", BackUrl = "/Account/" };
            try
            {
                string oldPwd = FytRequest.GetFormString("LoginPwd");
                var temo = GetSession();
                var user = OperateContext<tb_User>.SetServer.GetModel(m => m.ID == temo.ID);
                user.LoginPwd = UtilsHelper.MD5(oldPwd, true);
                OperateContext<tb_User>.SetServer.Update(user);
            }
            catch (Exception ex)
            {
                jsonM.Status = "err";
                jsonM.Msg = "发生错误！ 消息：" + ex.Message;
                LogHelper.WriteLog(typeof(AccountController), ex);
            }
            return Json(jsonM);
        }

        #region 消息管理
        /// <summary>
        /// 站内消息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Message()
        {
            int page = FytRequest.GetQueryInt("page", 1),
                pageSize = 10;
            var types = FytRequest.GetQueryString("types");//获取消息类型
            var user = GetSession();//获取用户信息
            var lq = from m in OperateSession.SetContext.lv_Message
                     orderby m.AddTime descending
                     select new
                     {
                         m.ID,
                         m.GoUserId,
                         GName = OperateSession.SetContext.tb_User.Where(u => u.ID == m.GoUserId).FirstOrDefault().NickName,
                         m.SendUserId,
                         SName = m.tb_User.NickName,
                         m.IsRead,
                         m.Centents,
                         m.AddTime
                     };
            if (types == "post")//发出的消息
            {
                lq = lq.Where(m => m.SendUserId == user.ID);
            }
            else//收到的消息
            {
                lq = lq.Where(m => m.GoUserId == user.ID);
            }
            ViewBag.list = JsonConverter.JsonClass(lq.Skip(pageSize * (page - 1)).Take(pageSize).ToList());
            ViewBag.pageModel = new PageHelper()
            {
                PageSize = pageSize,
                PageIndex = page,
                Rows = lq.Count(),
                Counts = Convert.ToInt32(Math.Ceiling((lq.Count() * 1.0) / pageSize)),
                Urls = "/account/message?page=" + page
            };
            ViewBag.nid = user.ID;
            return View();
        }

        /// <summary>
        /// 删除消息
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteMessage()
        {
            var jsonM = new JsonHelper.JsonAjaxModel() { Status = "y", Msg = "删除成功" };
            try
            {
                var vals = FytRequest.GetFormString("vals");
                var types = FytRequest.GetFormString("types");
                var temo = GetSession();
                if (!vals.Contains(","))
                {
                    var id = Convert.ToInt32(vals);
                    OperateContext<lv_Message>.SetServer.DeleteBy(m => m.ID == id && m.SendUserId == temo.ID);
                }
                else
                {
                    List<int> result = new List<string>(vals.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)).ConvertAll(int.Parse);
                    OperateContext<lv_Message>.SetServer.DeleteBy(m => result.Contains(m.ID) && m.SendUserId == temo.ID);
                }
                jsonM.Msg = types;
            }
            catch (Exception ex)
            {
                jsonM.Status = "err";
                jsonM.Msg = "删除数据发生错误！ 消息：" + ex.Message;
                LogHelper.WriteLog(typeof(AccountController), ex);
            }
            return Json(jsonM);
        }

        /// <summary>
        /// 设置已读信息
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SetRead()
        {
            var jsonM = new JsonHelper.JsonAjaxModel() { Status = "y", Msg = "修改成功" };
            try
            {
                var mid = FytRequest.GetFormInt("mid");//信息ID
                var types = FytRequest.GetFormString("mtypes");
                var user = GetSession();//获取用户信息
                var model = OperateContext<lv_Message>.SetServer.GetModel(m => m.ID == mid && m.GoUserId == user.ID);
                if (model != null)
                {
                    model.IsRead = true;
                    OperateContext<lv_Message>.SetServer.Update(model);
                }
                else
                {
                    jsonM.Status = "err";
                    jsonM.Msg = "操作异常，请刷新重试！";
                }
                jsonM.Msg = types;
            }
            catch (Exception ex)
            {
                jsonM.Status = "err";
                jsonM.Msg = "修改数据发生错误！ 消息：" + ex.Message;
                LogHelper.WriteLog(typeof(AccountController), ex);
            }
            return Json(jsonM);
        }

        /// <summary>
        /// 全部设置为已读
        /// </summary>
        /// <returns></returns>
        public ActionResult SetAll()
        {
            var jsonM = new JsonHelper.JsonAjaxModel() { Status = "y", Msg = "修改成功" };
            try
            {
                var types = FytRequest.GetFormString("types");
                var temo = GetSession();
                var list = OperateContext<lv_Message>.SetServer.GetList(m => m.GoUserId == temo.ID, m => m.ID, true).ToList();
                foreach (var item in list)
                {
                    item.IsRead = true;
                }
                OperateContext<lv_Message>.SetServer.Update(list);
                jsonM.Msg = types;
            }
            catch (Exception ex)
            {
                jsonM.Status = "err";
                jsonM.Msg = "修改数据发生错误！ 消息：" + ex.Message;
                LogHelper.WriteLog(typeof(AccountController), ex);
            }
            return Json(jsonM);
        }

        #endregion

        #region 我等你管理
        /// 我的活动
        /// </summary>
        /// <returns></returns>
        /// <summary>
        [HttpGet]
        public ActionResult MyProject()
        {
            int page = FytRequest.GetQueryInt("page", 1),
                pageSize = 10;
            var user = GetSession();//获取用户信息
            var lq = from p in OperateSession.SetContext.lv_ProJect
                     where p.UserId == user.ID
                     orderby p.UpdateTime descending
                     select new
                     {
                         p.ID,
                         p.UserId,
                         p.tb_User.NickName,
                         p.tb_User.TrueName,
                         p.tb_User.Types,
                         p.tb_User.HeadPic,
                         p.Title,
                         p.ShowImg,
                         p.Region,
                         p.Audit,
                         p.Rsum,
                         JoinNum = OperateSession.SetContext.lv_ProjectOrder.Where(m => m.ProJectId == p.ID).Count(),
                         p.AddTime,
                         p.UpdateTime
                     };
            ViewBag.list = JsonConverter.JsonClass(lq.Skip(pageSize * (page - 1)).Take(pageSize).ToList());
            ViewBag.pageModel = new PageHelper()
            {
                PageSize = pageSize,
                PageIndex = page,
                Rows = lq.Count(),
                Counts = Convert.ToInt32(Math.Ceiling((lq.Count() * 1.0) / pageSize)),
                Urls = "/account/myproject?page=" + page
            };
            return View();
        }

        /// <summary>
        /// 删除我等你
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteProject()
        {
            var jsonM = new JsonHelper.JsonAjaxModel() { Status = "y", Msg = "删除成功" };
            try
            {
                var vals = FytRequest.GetFormString("vals");
                if (!vals.Contains(","))
                {
                    var id = Convert.ToInt32(vals);
                    var count = OperateContext<lv_ProjectOrder>.SetServer.Count(m => m.ProJectId == id);
                    if (count > 1)
                    {
                        jsonM.Status = "err";
                        jsonM.Msg = "该活动已有人参与，不能删除";
                    }
                    else
                    {
                        OperateContext<lv_ProjectOrder>.SetServer.DeleteBy(m => m.ProJectId == id);
                        OperateContext<lv_ProJect>.SetServer.DeleteBy(m => m.ID == id);
                    }
                }
                else
                {
                    List<int> result = new List<string>(vals.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)).ConvertAll(int.Parse);
                    OperateContext<lv_ProjectOrder>.SetServer.DeleteBy(m => result.Contains(m.ProJectId));
                    OperateContext<lv_ProJect>.SetServer.DeleteBy(m => result.Contains(m.ID));
                }
            }
            catch (Exception ex)
            {
                jsonM.Status = "err";
                jsonM.Msg = "删除数据发生错误！ 消息：" + ex.Message;
                LogHelper.WriteLog(typeof(AccountController), ex);
            }
            return Json(jsonM);
        }

        /// <summary>
        /// 用户修改我等你完成状态
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ChangeStatus()
        {
            var jsonM = new JsonHelper.JsonAjaxModel() { Status = "y", Msg = "修改成功" };
            try
            {
                var gid = FytRequest.GetFormInt("vals");
                if (gid != 0)
                {
                    var user = GetSession();//获取用户信息
                    var model = OperateContext<lv_ProjectOrder>.SetServer.GetModel(m => m.ID == gid && m.UserId == user.ID);
                    if (model != null)
                    {
                        model.Status = true;
                        OperateContext<lv_ProjectOrder>.SetServer.Update(model);
                    }
                }
                else
                {
                    jsonM.Status = "err";
                    jsonM.Msg = "操作异常，请刷新重试！";
                }
            }
            catch (Exception ex)
            {
                jsonM.Status = "err";
                jsonM.Msg = "修改数据发生错误！ 消息：" + ex.Message;
                LogHelper.WriteLog(typeof(AccountController), ex);
            }
            return Json(jsonM);
        }

        #endregion

        #region 去看看管理
        /// <summary>
        /// 我发布的
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult MyActivity()
        {
            int page = FytRequest.GetQueryInt("page", 1),
                pageSize = 10;
            var user = GetSession();//获取用户信息
            var lq = from p in OperateSession.SetContext.lv_GoLook
                     where p.UserId == user.ID
                     orderby p.UpdateTime descending
                     select new
                     {
                         p.ID,
                         p.UserId,
                         p.tb_User.NickName,
                         p.tb_User.TrueName,
                         p.tb_User.HeadPic,
                         p.Title,
                         p.CoverImg,
                         p.ShowImg,
                         p.Rsum,
                         JoinNum = (int?)OperateSession.SetContext.lv_GoLookOrder.Where(m => m.LookId == p.ID).Count() ?? 0,
                         p.GoAddress,
                         p.ArriveTime,
                         p.Audit,
                         p.Flags,
                         p.Hits,
                         p.Centents,
                         p.UpdateTime
                     };
            ViewBag.list = JsonConverter.JsonClass(lq.Skip(pageSize * (page - 1)).Take(pageSize).ToList());
            ViewBag.pageModel = new PageHelper()
            {
                PageSize = pageSize,
                PageIndex = page,
                Rows = lq.Count(),
                Counts = Convert.ToInt32(Math.Ceiling((lq.Count() * 1.0) / pageSize)),
                Urls = "/account/myactivity?page=" + page
            };
            return View();
        }

        /// <summary>
        /// 删除去看看
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteActivity()
        {
            var jsonM = new JsonHelper.JsonAjaxModel() { Status = "y", Msg = "删除成功" };
            try
            {
                var vals = FytRequest.GetFormString("vals");
                if (!vals.Contains(","))
                {
                    var id = Convert.ToInt32(vals);
                    var count = OperateContext<lv_GoLookOrder>.SetServer.Count(m => m.LookId == id);
                    if (count > 1)
                    {
                        jsonM.Status = "err";
                        jsonM.Msg = "该活动已有人参与，不能删除";
                    }
                    else
                    {
                        OperateContext<lv_GoLook>.SetServer.DeleteBy(m => m.ID == id);
                    }
                }
                else
                {
                    List<int> result = new List<string>(vals.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)).ConvertAll(int.Parse);
                    OperateContext<lv_GoLook>.SetServer.DeleteBy(m => result.Contains(m.ID));
                }
            }
            catch (Exception ex)
            {
                jsonM.Status = "err";
                jsonM.Msg = "删除数据发生错误！ 消息：" + ex.Message;
                LogHelper.WriteLog(typeof(AccountController), ex);
            }
            return Json(jsonM);
        }

        /// <summary>
        /// 发布人取消用户
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ExitGolook()
        {
            var jsonM = new JsonHelper.JsonAjaxModel() { Status = "y", Msg = "取消成功" };
            try
            {
                var vals = FytRequest.GetFormString("vals");
                if (!string.IsNullOrEmpty(vals))
                {
                    var user = GetSession();//获取用户信息
                    if (!vals.Contains(','))
                    {
                        var id = Convert.ToInt32(vals);
                        var model = OperateContext<lv_GoLook>.SetServer.GetModel(m => m.ID == id);
                        if (model != null)
                        {
                            if (model.UserId == user.ID)
                            {
                                jsonM.Status = "err";
                                jsonM.Msg = "发起者不能取消自己";
                            }
                            else
                            {
                                OperateContext<lv_GoLookOrder>.SetServer.DeleteBy(m => m.LookId == id && m.UserId == user.ID);
                            }
                        }
                    }
                    else
                    {
                        List<int> result = new List<string>(vals.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)).ConvertAll(int.Parse);
                        if (!result.Contains(user.ID))
                        {
                            OperateContext<lv_GoLookOrder>.SetServer.DeleteBy(m => result.Contains(m.ID));
                        }
                        else
                        {
                            jsonM.Status = "err";
                            jsonM.Msg = "发起者不能取消自己";
                        }
                    }
                }
                else
                {
                    jsonM.Status = "err";
                    jsonM.Msg = "操作异常，请刷新重试！";
                }
            }
            catch (Exception ex)
            {
                jsonM.Status = "err";
                jsonM.Msg = "删除数据发生错误！ 消息：" + ex.Message;
                LogHelper.WriteLog(typeof(AccountController), ex);
            }
            return Json(jsonM);
        }

        /// <summary>
        /// 用户修改去看看类型
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ChangeFlag()
        {
            var jsonM = new JsonHelper.JsonAjaxModel() { Status = "y", Msg = "修改成功" };
            try
            {
                var gid = FytRequest.GetFormInt("gid");
                if (gid != 0)
                {
                    var user = GetSession();//获取用户信息
                    var model = OperateContext<lv_GoLook>.SetServer.GetModel(m => m.ID == gid && m.UserId == user.ID);
                    if (model != null)
                    {
                        model.Flags = model.Flags == 0 ? 1 : 0;
                        OperateContext<lv_GoLook>.SetServer.Update(model);
                    }
                }
                else
                {
                    jsonM.Status = "err";
                    jsonM.Msg = "操作异常，请刷新重试！";
                }
            }
            catch (Exception ex)
            {
                jsonM.Status = "err";
                jsonM.Msg = "修改数据发生错误！ 消息：" + ex.Message;
                LogHelper.WriteLog(typeof(AccountController), ex);
            }
            return Json(jsonM);
        }

        #endregion

        #region 旅程故事管理
        /// <summary>
        /// 我的旅行故事
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult MyStory()
        {
            int page = FytRequest.GetQueryInt("page", 1),
                pageSize = 10;
            var user = GetSession();//获取用户信息
            var lq = from p in OperateSession.SetContext.lv_Story
                     where p.UserId == user.ID
                     orderby p.UpdateTime descending
                     select new
                     {
                         p.ID,
                         p.UserId,
                         p.Title,
                         p.UpdateTime,
                         p.AddTime
                     };
            ViewBag.list = JsonConverter.JsonClass(lq.Skip(pageSize * (page - 1)).Take(pageSize).ToList());
            ViewBag.pageModel = new PageHelper()
            {
                PageSize = pageSize,
                PageIndex = page,
                Rows = lq.Count(),
                Counts = Convert.ToInt32(Math.Ceiling((lq.Count() * 1.0) / pageSize)),
                Urls = "/account/mystory?page=" + page
            };
            return View();
        }

        /// <summary>
        /// 删除我的旅程故事
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteStory()
        {
            var jsonM = new JsonHelper.JsonAjaxModel() { Status = "y", Msg = "删除成功" };
            try
            {
                var vals = FytRequest.GetFormString("vals");
                if (!vals.Contains(","))
                {
                    var id = Convert.ToInt32(vals);
                    OperateContext<lv_Story>.SetServer.DeleteBy(m => m.ID == id);
                }
                else
                {
                    List<int> result = new List<string>(vals.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)).ConvertAll(int.Parse);
                    OperateContext<lv_Story>.SetServer.DeleteBy(m => result.Contains(m.ID));
                }
            }
            catch (Exception ex)
            {
                jsonM.Status = "err";
                jsonM.Msg = "删除数据发生错误！ 消息：" + ex.Message;
                LogHelper.WriteLog(typeof(AccountController), ex);
            }
            return Json(jsonM);
        }

        #endregion

        #region 我参与的管理
        /// <summary>
        /// 我参与的
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult MyJoin()
        {
            int page = FytRequest.GetQueryInt("page", 1),
                pageSize = 10;
            var types = FytRequest.GetQueryInt("types", 0);
            var user = GetSession();//获取用户信息
            if (types == 0)
            {
                //我等你
                var lq = from po in OperateSession.SetContext.lv_ProjectOrder
                         join p in OperateSession.SetContext.lv_ProJect on po.ProJectId equals p.ID
                         where po.UserId == user.ID && p.UserId != user.ID
                         orderby p.UpdateTime descending
                         select new
                         {
                             po.ID,
                             po.PayStatus,
                             po.Status,
                             po.AddTime,
                             po.OrderDate,
                             ProjectId = p.ID,
                             p.UserId,
                             p.tb_User.NickName,
                             p.tb_User.TrueName,
                             p.tb_User.Types,
                             p.tb_User.HeadPic,
                             p.Title,
                             p.Region,
                             p.Rsum,
                             p.UpdateTime
                         };
                ViewBag.list = JsonConverter.JsonClass(lq.Skip(pageSize * (page - 1)).Take(pageSize).ToList());
                ViewBag.pageModel = new PageHelper()
                {
                    PageSize = pageSize,
                    PageIndex = page,
                    Rows = lq.Count(),
                    Counts = Convert.ToInt32(Math.Ceiling((lq.Count() * 1.0) / pageSize)),
                    Urls = "/account/myjoin?page=" + page + "&types=" + types
                };
            }
            else
            {
                //去看看
                var lq = from go in OperateSession.SetContext.lv_GoLookOrder
                         join p in OperateSession.SetContext.lv_GoLook on go.LookId equals p.ID
                         where go.UserId == user.ID && p.Flags == 1
                         orderby p.UpdateTime descending
                         select new
                         {
                             go.ID,
                             go.PayStatus,
                             GoLookId = p.ID,
                             p.UserId,
                             p.tb_User.NickName,
                             p.tb_User.TrueName,
                             p.tb_User.HeadPic,
                             ProjectId = go.ID,
                             p.Title,
                             p.CoverImg,
                             p.ShowImg,
                             p.Rsum,
                             JoinNum = (int?)OperateSession.SetContext.lv_GoLookOrder.Where(m => m.LookId == p.ID).Count() ?? 0,
                             p.GoAddress,
                             p.ArriveTime,
                             p.Flags,
                             p.Hits,
                             p.Centents,
                             p.UpdateTime
                         };
                ViewBag.list = JsonConverter.JsonClass(lq.Skip(pageSize * (page - 1)).Take(pageSize).ToList());
                ViewBag.pageModel = new PageHelper()
                {
                    PageSize = pageSize,
                    PageIndex = page,
                    Rows = lq.Count(),
                    Counts = Convert.ToInt32(Math.Ceiling((lq.Count() * 1.0) / pageSize)),
                    Urls = "/account/myjoin?page=" + page + "&types=" + types
                };
            }
            return View();
        }
        #endregion

        #region 我的收藏管理
        /// <summary>
        /// 我的收藏
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult MyCollect()
        {
            int page = FytRequest.GetQueryInt("page", 1),
                   pageSize = 10;
            var user = GetSession();//获取用户信息
            var lq = from c in OperateSession.SetContext.tb_Collect
                     join p in OperateSession.SetContext.lv_ProJect on c.CollectId equals p.ID
                     where c.UserId == user.ID
                     orderby p.UpdateTime descending
                     select new
                     {
                         c.ID,
                         ProjectId = p.ID,
                         p.UserId,
                         p.tb_User.NickName,
                         p.tb_User.TrueName,
                         p.tb_User.Types,
                         p.tb_User.HeadPic,
                         p.Title,
                         p.Region,
                         p.Rsum,
                         p.AddTime,
                         p.UpdateTime
                     };
            ViewBag.list = JsonConverter.JsonClass(lq.Skip(pageSize * (page - 1)).Take(pageSize).ToList());
            ViewBag.pageModel = new PageHelper()
            {
                PageSize = pageSize,
                PageIndex = page,
                Rows = lq.Count(),
                Counts = Convert.ToInt32(Math.Ceiling((lq.Count() * 1.0) / pageSize)),
                Urls = "/account/mycollect?page=" + page
            };
            return View();
        }

        /// <summary>
        /// 删除我的收藏
        /// </summary>
        /// <returns></returns>
        public ActionResult DeleteCollect()
        {
            var jsonM = new JsonHelper.JsonAjaxModel() { Status = "y", Msg = "删除成功" };
            try
            {
                var vals = FytRequest.GetFormString("vals");
                if (!vals.Contains(","))
                {
                    var id = Convert.ToInt32(vals);
                    OperateContext<tb_Collect>.SetServer.DeleteBy(m => m.ID == id);
                }
                else
                {
                    List<int> result = new List<string>(vals.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)).ConvertAll(int.Parse);
                    OperateContext<tb_Collect>.SetServer.DeleteBy(m => result.Contains(m.ID));
                }
            }
            catch (Exception ex)
            {
                jsonM.Status = "err";
                jsonM.Msg = "删除数据发生错误！ 消息：" + ex.Message;
                LogHelper.WriteLog(typeof(AccountController), ex);
            }
            return Json(jsonM);
        }
        #endregion

        #region 我的财务管理
        /// <summary>
        /// 我的财务
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult MyFinancial()
        {
            int page = FytRequest.GetQueryInt("page", 1),
                   pageSize = 10;
            var user = GetSession();//获取用户信息
            ViewBag.useramount = user.Amount;
            var lq = from u in OperateSession.SetContext.tb_User
                     join uml in OperateSession.SetContext.tb_UserMoneyLog on u.ID equals uml.UserId
                     where uml.UserId == user.ID
                     orderby uml.AddDate descending
                     select new
                     {
                         uml.ID,
                         uml.Option,
                         uml.Price,
                         uml.Summary,
                         uml.AddDate
                     };
            ViewBag.list = JsonConverter.JsonClass(lq.Skip(pageSize * (page - 1)).Take(pageSize).ToList());
            ViewBag.pageModel = new PageHelper()
            {
                PageSize = pageSize,
                PageIndex = page,
                Rows = lq.Count(),
                Counts = Convert.ToInt32(Math.Ceiling((lq.Count() * 1.0) / pageSize)),
                Urls = "/account/myfinancial?page=" + page
            };
            return View();
        }

        /// <summary>
        /// 我的提现记录
        /// </summary>
        /// <returns></returns>
        public ActionResult MySetMoney()
        {
            int page = FytRequest.GetQueryInt("page", 1),
                     pageSize = 10;
            var user = GetSession();//获取用户信息
            ViewBag.useramount = user.Amount;
            var lq = from u in OperateSession.SetContext.tb_User
                     join gm in OperateSession.SetContext.lv_GetMoney on u.ID equals gm.UserId
                     join b in OperateSession.SetContext.lv_Bank on gm.BankId equals b.ID
                     where gm.UserId == user.ID
                     orderby gm.AddTime descending
                     select new
                     {
                         u.TrueName,
                         gm.Account,
                         gm.Price,
                         b.BankAccont,
                         gm.Status,

                     };
            ViewBag.list = JsonConverter.JsonClass(lq.Skip(pageSize * (page - 1)).Take(pageSize).ToList());
            ViewBag.pageModel = new PageHelper()
            {
                PageSize = pageSize,
                PageIndex = page,
                Rows = lq.Count(),
                Counts = Convert.ToInt32(Math.Ceiling((lq.Count() * 1.0) / pageSize)),
                Urls = "/account/mysetmoney?page=" + page
            };
            return View();
        }

        /// <summary>
        /// 提现
        /// </summary>
        /// <returns></returns>
        public ActionResult SetMoneyOut()
        {
            var user = GetSession();
            ViewBag.user = user;
            var bankSelect = OperateContext<lv_Bank>.SetServer.GetList(m => m.UserId == user.ID, m => m.AddTime, false).ToList();
            var selBank = bankSelect.Select(s => new SelectListItem
            {
                Text = s.UserName + " " + s.BankName + " " + s.BankAccont,
                Value = s.ID.ToString(CultureInfo.InvariantCulture)
            }).ToList();
            selBank.Insert(0, new SelectListItem() { Text = "请选择提现账号", Value = "0" });
            ViewBag.selBank = selBank;
            return View();
        }

        /// <summary>
        /// 用户提现
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UserSetMoneyOut()
        {
            var jsonM = new JsonHelper.JsonAjaxModel() { Status = "y", Msg = "提现成功", BackUrl = "/account/mysetmoney" };
            try
            {
                var bankId = FytRequest.GetFormInt("bankId");
                var areanum = FytRequest.GetFormString("areanum");
                var mobile = FytRequest.GetFormString("mobile");
                var setmoney = FytRequest.GetFormDecimal("setmoney", 0);
                if (bankId != 0)
                {
                    if (setmoney != 0)
                    {
                        var user = GetSession();
                        if (setmoney <= user.Amount)
                        {
                            //修改账户余额
                            user.Amount = user.Amount - setmoney;
                            OperateContext<tb_User>.SetServer.Update(user);
                            //录入财务信息
                            var uml = new tb_UserMoneyLog()
                            {
                                UserId = user.ID,
                                Number = UtilsHelper.GetRamCode(),
                                Option = 2,
                                Price = setmoney,
                                Status = 1,
                                PayType = "",
                                Summary = "提现",
                                AddDate = DateTime.Now
                            };
                            OperateContext<tb_UserMoneyLog>.SetServer.Add(uml);
                            //录入提现申请
                            var gm = new lv_GetMoney()
                            {
                                Number = UtilsHelper.Number(9, false),
                                UserId = user.ID,
                                Price = setmoney,
                                BankId = bankId,
                                AreaNum = areanum,
                                Account = mobile,
                                Status = false,
                                AddTime = DateTime.Now
                            };
                            OperateContext<lv_GetMoney>.SetServer.Add(gm);
                            if (HttpContext.Session != null) HttpContext.Session["FytUser"] = user;
                        }
                        else
                        {
                            jsonM.Status = "err";
                            jsonM.Msg = "提现金额不能大于当前余额";
                        }
                    }
                    else
                    {
                        jsonM.Status = "err";
                        jsonM.Msg = "提现金额不能为0";
                    }
                }
                else
                {
                    jsonM.Status = "err";
                    jsonM.Msg = "请先选择提现账号";
                }
            }
            catch (Exception ex)
            {
                jsonM.Status = "err";
                jsonM.Msg = "编辑数据发生错误！ 消息：" + ex.Message;
                LogHelper.WriteLog(typeof(AccountController), ex);
            }
            return Json(jsonM);
        }

        /// <summary>
        /// 我的银行卡
        /// </summary>
        /// <returns></returns>
        public ActionResult MyBankcard()
        {
            int page = FytRequest.GetQueryInt("page", 1),
                   pageSize = 10;
            var user = GetSession();//获取用户信息
            ViewBag.useramount = user.Amount;
            var lq = from b in OperateSession.SetContext.lv_Bank
                     where b.UserId == user.ID
                     orderby b.AddTime descending
                     select new
                     {
                         b.ID,
                         b.UserName,
                         b.BankName,
                         b.BankAccont
                     };
            ViewBag.list = JsonConverter.JsonClass(lq.Skip(pageSize * (page - 1)).Take(pageSize).ToList());
            ViewBag.pageModel = new PageHelper()
            {
                PageSize = pageSize,
                PageIndex = page,
                Rows = lq.Count(),
                Counts = Convert.ToInt32(Math.Ceiling((lq.Count() * 1.0) / pageSize)),
                Urls = "/account/mybankcard?page=" + page
            };
            return View();
        }

        /// <summary>
        /// 编辑银行卡
        /// </summary>
        /// <returns></returns>
        public ActionResult EditBankcard()
        {
            var bankId = FytRequest.GetQueryInt("bankId");//银行卡ID
            var model = new lv_Bank() { UserName = "", BankName = "", BankAccont = "", BankAddress = "" };
            if (bankId != 0)
            {
                model = OperateContext<lv_Bank>.SetServer.GetModel(m => m.ID == bankId);
            }
            ViewBag.bank = model;
            return View();
        }

        /// <summary>
        /// 用户编辑银行卡
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UserEditBankcard()
        {
            var jsonM = new JsonHelper.JsonAjaxModel() { Status = "y", Msg = "修改成功" };
            try
            {
                int bankId = FytRequest.GetFormInt("bankId");
                var user = GetSession();
                string userName = FytRequest.GetFormString("UserName"),
                       bankName = FytRequest.GetFormString("BankName"),
                       bankAccount = FytRequest.GetFormString("BankAccont");
                if (bankId != 0)
                {
                    //修改银行卡
                    var model = OperateContext<lv_Bank>.SetServer.GetModel(m => m.ID == bankId && m.UserId == user.ID);
                    if (model != null)
                    {
                        model.UserName = userName;
                        model.BankName = bankName;
                        model.BankAddress = "";
                        if (!bankAccount.Contains("*"))
                        {
                            model.BankAccont = bankAccount;
                        }
                        OperateContext<lv_Bank>.SetServer.Update(model);
                    }
                }
                else
                {
                    //新增银行卡
                    var model = new lv_Bank()
                    {
                        UserId = user.ID,
                        UserName = userName,
                        BankName = bankName,
                        BankAddress = "",
                        BankAccont = bankAccount,
                        AddTime = DateTime.Now
                    };
                    OperateContext<lv_Bank>.SetServer.Add(model);
                }
                jsonM.BackUrl = "/account/mybankcard";
            }
            catch (Exception ex)
            {
                jsonM.Status = "err";
                jsonM.Msg = "编辑数据发生错误！ 消息：" + ex.Message;
                LogHelper.WriteLog(typeof(AccountController), ex);
            }
            return Json(jsonM);
        }

        /// <summary>
        /// 删除银行卡
        /// </summary>
        /// <returns></returns>
        public ActionResult DeleteBankcard()
        {
            var jsonM = new JsonHelper.JsonAjaxModel() { Status = "y", Msg = "删除成功" };
            try
            {
                var user = GetSession();
                var vals = FytRequest.GetFormString("vals");
                if (!vals.Contains(","))
                {
                    var id = Convert.ToInt32(vals);
                    OperateContext<lv_Bank>.SetServer.DeleteBy(m => m.ID == id && m.UserId == user.ID);
                }
                else
                {
                    List<int> result = new List<string>(vals.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)).ConvertAll(int.Parse);
                    OperateContext<lv_Bank>.SetServer.DeleteBy(m => result.Contains(m.ID) && m.UserId == user.ID);
                }
            }
            catch (Exception ex)
            {
                jsonM.Status = "err";
                jsonM.Msg = "删除数据发生错误！ 消息：" + ex.Message;
                LogHelper.WriteLog(typeof(AccountController), ex);
            }
            return Json(jsonM);
        }

        #endregion

        #region 取消订单 + CancelOrder()
        /// <summary>
        /// 取消订单
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CancelOrder()
        {
            var jsonM = new JsonHelper.JsonAjaxModel() { Status = "y", Msg = "成功" };
            try
            {
                int orderId = FytRequest.GetFormInt("orderId"),//要取消的订单ID
                    types = FytRequest.GetFormInt("types");//订单类型 0：我等你/1：去看看
                var user = GetSession();
                if (types == 0)
                {
                    OperateContext<lv_ProjectOrder>.SetServer.DeleteBy(m => m.ID == orderId && m.UserId == user.ID);
                }
                else if (types == 1)
                {
                    OperateContext<lv_GoLookOrder>.SetServer.DeleteBy(m => m.ID == orderId && m.UserId == user.ID);
                }
            }
            catch (Exception ex)
            {
                jsonM.Status = "err";
                jsonM.Msg = "发生错误！ 消息：" + ex.Message;
                LogHelper.WriteLog(typeof(AccountController), ex);
            }
            return Json(jsonM);
        }
        #endregion

        #region 支付失败 + PayFailed()
        /// <summary>
        /// 支付失败
        /// </summary>
        /// <returns></returns>
        public ActionResult PayFailed()
        {
            var o = FytRequest.GetQueryString("o");//订单号
            var header = o.Substring(0, 2);
            if (header == "WD")
            {
                OperateContext<lv_ProjectOrder>.SetServer.DeleteBy(m => m.Number == o);
            }
            else if (header == "QK")
            {
                OperateContext<lv_GoLookOrder>.SetServer.DeleteBy(m => m.Number == o);
            }
            return View();
        }
        #endregion

        /// <summary>
        /// 左侧导航
        /// </summary>
        /// <returns></returns>
        public ActionResult LeftNav()
        {
            return View();
        }

        public ActionResult GetMessCount()
        {
            var jsonM = new JsonHelper.JsonAjaxModel() { Data = 0 };
            var user = GetSession();
            jsonM.Data = OperateContext<lv_Message>.SetServer.Count(m => m.GoUserId == user.ID && m.IsRead == false);
            return Json(jsonM);
        }
    }
}