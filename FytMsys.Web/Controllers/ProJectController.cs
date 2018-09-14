﻿using Domain.Entity;
using Domain.ViewModel;
using FytMsys.Common;
using FytMsys.Helper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using JsonConverter = FytMsys.Common.JsonConverter;

namespace FytMsys.Web.Controllers
{
    /// <summary>
    /// 我等你管理
    /// </summary>
    public class ProJectController : Controller
    {
        #region 1、[我等你] 列表页 + Index()
        /// <summary>
        /// 列表页
        /// </summary>
        /// <returns></returns>
        public ActionResult Index(int orderinfo, int page)
        {
            int pageSize = 15;
            var lq = from p in OperateSession.SetContext.lv_ProJect
                     where p.Audit == 1
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
                         p.CoverImg,
                         p.Region,
                         p.Hits,
                         StarNum = (int?)OperateSession.SetContext.tb_Comment.Where(m => m.ClassId == p.ID && m.Option == 1).Sum(m => m.Star) % OperateSession.SetContext.tb_Comment.Count(m => m.ClassId == p.ID) ?? 0,
                         p.AddTime,
                         p.UpdateTime
                     };
            switch (orderinfo)
            {
                case 0: lq = lq.OrderByDescending(m => m.AddTime); break;
                case 1: lq = lq.OrderBy(m => m.AddTime); break;
                case 2: lq = lq.OrderByDescending(m => m.Hits); break;
                case 3: lq = lq.OrderBy(m => m.Hits); break;
            }
            ViewBag.list = JsonConverter.JsonClass(lq.Skip(pageSize * (page - 1)).Take(pageSize).ToList());
            //分页控件
            ViewBag.pageModel = new PageHelper()
            {
                PageSize = pageSize,
                PageIndex = page,
                Rows = lq.Count(),
                Counts = Convert.ToInt32(Math.Ceiling((lq.Count() * 1.0) / pageSize)),
                Urls = "/project/index/" + orderinfo + "-" + page
            };
            //网站信息
            var siteModel = WebSiteHelper.GetSite(1);
            //广告位
            var banner = OperateContext<tb_AdvList>.SetServer.GetList(m => m.ClassId == 5, m => m.Sort, false).Where(m => m.Status).FirstOrDefault();
            ViewBag.banner = banner;
            return View(siteModel);
        }
        #endregion

        #region 2、[我等你] 发布我等你信息 + Publish()
        /// <summary>
        /// 发布我等你信息
        /// </summary>
        /// <returns></returns>
        public ActionResult Publish()
        {
            var userId = 0;
            if (!string.IsNullOrEmpty(UtilsHelper.GetCookie("FytUserId")))
            {
                userId = Convert.ToInt32(DESEncrypt.Decrypt(UtilsHelper.GetCookie("FytUserId")));
                var user = OperateContext<tb_User>.SetServer.GetModel(m => m.ID == userId);
                ViewBag.user = user;
            }
            var model = new lv_ProJect() {Price ="0"};
            var pid = FytRequest.GetQueryInt("pid");
            if (pid > 0)
            {
                model = OperateContext<lv_ProJect>.SetServer.GetModel(m => m.ID == pid);
                ViewBag.imglist = OperateContext<tb_ImageLibrary>.SetServer.GetList(m => m.ImgID == pid && m.ImgType == 2, m => m.ID, true).ToList();
            }
     
              return View(model);          
        }
        public ActionResult PublishEng()
        {
            var userId = 0;
            if (!string.IsNullOrEmpty(UtilsHelper.GetCookie("FytUserId")))
            {
                userId = Convert.ToInt32(DESEncrypt.Decrypt(UtilsHelper.GetCookie("FytUserId")));
                var user = OperateContext<tb_User>.SetServer.GetModel(m => m.ID == userId);
                ViewBag.user = user;
            }
            var model = new lv_ProJect() { Price = "0" };
            var pid = FytRequest.GetQueryInt("pid");
            if (pid > 0)
            {
                model = OperateContext<lv_ProJect>.SetServer.GetModel(m => m.ID == pid);
                ViewBag.imglist = OperateContext<tb_ImageLibrary>.SetServer.GetList(m => m.ImgID == pid && m.ImgType == 2, m => m.ID, true).ToList();
            }

            return View(model);
        }
        #endregion

        #region 3、[我等你] 发布我等你信息 + Publish()
        /// <summary>
        /// 发布我等你信息
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Publish(lv_ProJect model)
        {
            var jsonM = new JsonHelper.JsonAjaxModel() { Status = "y", Msg = "success" };
            try
            {
                var mtype = FytRequest.GetFormString("pricetype");
                var rmb = "0";
                var usd = "0";
                if (mtype == "人民币")
                {
                    rmb = model.Price;
                    usd = Math.Round(Convert.ToDecimal(model.Price)/MoneyHelper.GetHl(), 2).ToString();
                }
                else
                {
                    //美元
                    usd = model.Price;
                    rmb= Math.Round(Convert.ToDecimal(model.Price) * MoneyHelper.GetHl(), 2).ToString();
                }
                //获取Cookie用户ID
                var userId = 0;
                if (!string.IsNullOrEmpty(UtilsHelper.GetCookie("FytUserId")))
                {
                    if (model.ID == 0)
                    {
                        userId = Convert.ToInt32(DESEncrypt.Decrypt(UtilsHelper.GetCookie("FytUserId")));
                        model.Number = UtilsHelper.GetRamCode();
                        model.UserId = userId;
                        model.IsRecommend = false;
                        model.IsSpecial = false;
                        model.AddTime = DateTime.Now;
                        model.UpdateTime = DateTime.Now;
                        model.OrderDate = DateTime.Now;
                        model.Price = rmb;
                        model.UsdPrice = usd;
                      
                        //保存我等你信息
                        OperateContext<lv_ProJect>.SetServer.Add(model);
                        //保存我等你参与记录
                        //var po = new lv_ProjectOrder()
                        //{
                        //    Number = UtilsHelper.GetRamCode(),
                        //    ProJectId = model.ID,
                        //    UserId = model.UserId,
                        //    PayPrice = Convert.ToDecimal(model.Price),
                        //    PayAccount = "",
                        //    PayName = "",
                        //    PayStatus = 1,
                        //    PeopleNum = 1,
                        //    Status = true,
                        //    AddTime = DateTime.Now
                        //};
                        //OperateContext<lv_ProjectOrder>.SetServer.Add(po);
                    }
                    else
                    {
                        var project = OperateContext<lv_ProJect>.SetServer.GetModel(m => m.ID == model.ID);
                        project.Title = model.Title;
                        project.Price = rmb;
                        project.UsdPrice = usd;
                        project.Rsum = model.Rsum;
                        project.Centents = model.Centents;
                        project.Region = model.Region;
                        project.IsTcjs = model.IsTcjs;
                        project.IsJcjs = model.IsJcjs;
                        project.IsApzs = model.IsApzs;
                        project.CoverImg = model.CoverImg;
                        project.ShowImg = model.ShowImg;
                        project.UpdateTime = DateTime.Now;
                        OperateContext<lv_ProJect>.SetServer.Update(project);
                        //删除旧图片
                        OperateContext<tb_ImageLibrary>.SetServer.DeleteBy(m => m.ImgID == project.ID && m.ImgType == 2);
                    }
                    #region 多张图片上传
                    var reslist = FytRequest.GetFormString("imlist").Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    var listModel = new List<tb_ImageLibrary>();
                    foreach (string s in reslist)
                    {
                        var u = FytRequest.GetFormString("file_name_" + s);
                        if (u != "")
                        {
                            listModel.Add(new tb_ImageLibrary()
                            {
                                ImgID = model.ID,
                                ImgType = 2,
                                ImgUrl = u,
                                ImgSmall = u,
                                IsCover = false
                            });
                        }
                    }
                    OperateContext<tb_ImageLibrary>.SetServer.AddEntity(listModel);
                    #endregion
                    jsonM.Msg = model.ID.ToString();
                }
                else
                {
                    jsonM.Status = "err";
                    jsonM.Msg = "您的登录信息已失效,请先登录！";
                }
            }
            catch (Exception ex)
            {
                jsonM.Status = "err";
                jsonM.Msg = "修改数据发生错误！ 消息：" + ex.Message;
                LogHelper.WriteLog(typeof(ProJectController), ex);
            }
            return Json(jsonM);
        }
        #endregion

        #region 4、[我等你] 详情 + Detail(int id)
        /// <summary>
        /// 详情
        /// </summary>
        /// <returns></returns>
        public ActionResult Detail(int id)
        {
            //获取我等你详情
            var model = OperateContext<lv_ProJect>.SetServer.GetModel(m => m.ID == id);
            //获取我等你图片列表
            var imglist = OperateContext<tb_ImageLibrary>.SetServer.GetList(
                PredicateBuilder.True<tb_ImageLibrary>().And(m => m.ImgID == id).And(m => m.ImgType == 2),
                m => m.IsCover, true);
            ViewBag.imglist = imglist.ToList();
            //获取Cookie用户ID
            var userId = 0;
            if (!string.IsNullOrEmpty(UtilsHelper.GetCookie("FytUserId")))
            {
                userId = Convert.ToInt32(DESEncrypt.Decrypt(UtilsHelper.GetCookie("FytUserId")));
            }
            //获取我等你参与人列表
            var peoplelist = OperateContext<lv_ProjectOrder>.SetServer.GetList(m => m.ProJectId == id, m => m.AddTime, true).ToList();
            ViewBag.peoplelist = peoplelist;
            //获取当前登录用户
            if (userId != 0)
            {
                var ToUser = OperateContext<tb_User>.SetServer.GetModel(m => m.ID == userId);
                ViewBag.UserSex = ToUser.Sex;
            }
            ViewBag.User = userId;
            
            //判断用户是否参与
            var isExist = 0;
            foreach (var item in peoplelist.ToList())
            {
                if (item.UserId == userId)
                {
                    isExist = 1;
                }
            }
            ViewBag.isExist = isExist;
            //获取我等你评论列表
            var commentlist = OperateContext<tb_Comment>.SetServer.GetList(
                PredicateBuilder.True<tb_Comment>().And(m => m.ClassId == id).And(m => m.Option == 1)
                , m => m.AddDate, false).ToList();
            ViewBag.commentlist = commentlist;
            //获取我等你评星
            var starnum = 0;
            if (commentlist != null && commentlist.Count > 0)
            {
                foreach (var item in commentlist)
                {
                    starnum += item.Star;
                }
                starnum = starnum % commentlist.Count;
            }
            else
            {
                starnum = 5;
            }
            ViewBag.starnum = starnum;
            //更新用户访问量
            model.Hits = model.Hits + 1;
            ViewBag.userId = userId;
            OperateContext<lv_ProJect>.SetServer.Update(model);
            return View(model);
        }
        #endregion

        #region 5、[我等你] 报名支付 + ProjectOrder()
        /// <summary>
        /// 报名支付
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ProjectOrder()
        {
            var jsonM = new JsonHelper.JsonAjaxModel() { Status = "y", Msg = "success" };
            var transactionOption = new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadCommitted,
                Timeout = new TimeSpan(0, 0, 60)
            };
            //设置事务隔离级别   设置事务超时时间为60秒
            using (var scope = new TransactionScope(TransactionScopeOption.Required, transactionOption))
            {
                try
                {
                    var projectId = FytRequest.GetFormInt("projectId");//我等你ID
                    var payType = FytRequest.GetFormString("payType");//支付类型
                    var PeopleNum = FytRequest.GetFormInt("PeopleNum");//参与人数
                    var usd = FytRequest.GetFormDecimal("husd",0); //所需支付的美元
                    var OrderDate = FytRequest.GetFormString("OrderDate");//预约日期
                    var info = "";
                    //获取Cookie用户ID
                    var userId = 0;
                    if (!string.IsNullOrEmpty(UtilsHelper.GetCookie("FytUserId")))
                    {
                        userId = Convert.ToInt32(DESEncrypt.Decrypt(UtilsHelper.GetCookie("FytUserId")));

                        var CurrentUser = OperateContext<tb_User>.SetServer.GetModel(m => m.ID == userId);
                        //获取我等你信息
                        var project = OperateContext<lv_ProJect>.SetServer.GetModel(m => m.ID == projectId);
                        //获取我等你在预约日期参与人数
                        var joinnum = OperateContext<lv_ProjectOrder>.SetServer.Count(m => m.ProJectId == projectId);
                        var Rsum = Int32.Parse(project.Rsum);
                        if (joinnum < Rsum)
                        {
                            var IsExist = OperateContext<lv_ProjectOrder>.SetServer.Exist(PredicateBuilder.True<lv_ProjectOrder>().And(m => m.ProJectId == projectId).And(m => m.UserId == userId));
                           
                                switch (payType)
                                {
                                    case "zfb": info = "支付宝"; break;
                                    case "paypal": info = "PayPal"; break;
                                    case "xyk": info = "信用卡"; break;
                                    default: info = "支付宝"; break;
                                }
                                //生成参与订单
                                var model = new lv_ProjectOrder()
                                {
                                    Number = "WD" + UtilsHelper.GetRamCode(),
                                    UserId = userId,
                                    ProJectId = projectId,
                                    PeopleNum = PeopleNum,
                                    OrderDate = OrderDate,
                                    Status = false,
                                    PayStatus = 0,
                                    PayName = info,
                                    PayPrice =payType== "zfb" ? Convert.ToDecimal(project.Price) * PeopleNum : Convert.ToDecimal(project.UsdPrice) * PeopleNum,
                                    PayAccount = "",
                                    AddTime = DateTime.Now
                                };
                                if (CurrentUser.Sex == "女")
                                    model.PayPrice = payType == "zfb"
                                        ? Convert.ToDecimal(project.Price)*PeopleNum*
                                          Convert.ToDecimal(project.LadyDiscount)
                                        : Convert.ToDecimal(project.UsdPrice)*PeopleNum*
                                          Convert.ToDecimal(project.LadyDiscount);
                                OperateContext<lv_ProjectOrder>.SetServer.Add(model);
                                jsonM.Data = model.Number;
                                jsonM.DataA = payType;
                                // 没有错误,提交事务
                                scope.Complete();
                            }
                            //else
                            //{
                            //    jsonM.Status = "n";
                            //    jsonM.Msg = "抱歉，您已报名!";
                            //}
                        
                        else
                        {
                            jsonM.Status = "n";
                            jsonM.Msg = "抱歉，当天行程已满!";
                        }
                    }
                    else
                    {
                        jsonM.Status = "ul";
                        jsonM.Msg = "您尚未登录，请先登录!";
                    }
                }
                catch (Exception ex)
                {
                    jsonM.Status = "err";
                    throw new Exception("发送信息异常,原因:" + ex.Message);
                }
                finally
                {
                    //释放资源
                    scope.Dispose();
                }
            }
            return Json(jsonM);
        }
        #endregion

        #region 6、[我等你] 支付完成 + PaySuccess()
        /// <summary>
        /// 支付完成
        /// </summary>
        /// <returns></returns>
        public ActionResult PaySuccess()
        {
            //我等你详情
            var ordernum = FytRequest.GetQueryString("o");
            var model = OperateContext<lv_ProjectOrder>.SetServer.GetModel(m => m.Number == ordernum);
            return View(model);
        }
        #endregion

        #region 7、[我等你] 收藏 + UserCollect()
        /// <summary>
        /// 收藏
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UserCollect()
        {
            var jsonM = new JsonHelper.JsonAjaxModel() { Status = "y", Msg = "收藏成功！" };
            try
            {
                int projectId = FytRequest.GetFormInt("proId");//我等你ID
                //获取Cookie用户ID
                var userId = 0;
                if (!string.IsNullOrEmpty(UtilsHelper.GetCookie("FytUserId")))
                {
                    userId = Convert.ToInt32(DESEncrypt.Decrypt(UtilsHelper.GetCookie("FytUserId")));
                    //判断用户是否已收藏
                    var isExist = OperateContext<tb_Collect>.SetServer.Count(
                        PredicateBuilder.True<tb_Collect>().And(m => m.CollectId == projectId).And(m => m.UserId == userId).And(m => m.CollectType == 1));
                    if (isExist == 0)
                    {
                        //加入收藏
                        var model = new tb_Collect()
                        {
                            CollectId = projectId,
                            UserId = userId,
                            CollectType = 1,
                            AddDate = DateTime.Now
                        };
                        OperateContext<tb_Collect>.SetServer.Add(model);
                    }
                    else
                    {
                        jsonM.Status = "err";
                        jsonM.Msg = "您已收藏该旅游";
                    }
                }
                else
                {
                    jsonM.Status = "unlogin";
                    jsonM.Msg = "用户未登录";
                }
            }
            catch (Exception ex)
            {
                jsonM.Status = "err";
                jsonM.Msg = "修改数据发生错误！ 消息：" + ex.Message;
                LogHelper.WriteLog(typeof(ProJectController), ex);
            }
            return Json(jsonM);
        }
        #endregion

        #region 8、[我等你] 发评论 + UserComment()
        /// <summary>
        /// 发评论
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UserComment(tb_Comment model)
        {
            var jsonM = new JsonHelper.JsonAjaxModel() { Status = "y", Msg = "评论成功！" };
            try
            {
                //获取Cookie用户ID
                var userId = 0;
                if (!string.IsNullOrEmpty(UtilsHelper.GetCookie("FytUserId")))
                {
                    userId = Convert.ToInt32(DESEncrypt.Decrypt(UtilsHelper.GetCookie("FytUserId")));
                    model.UserId = userId;
                    model.Option = 1;
                    model.Star = Convert.ToInt32(Request.Form["score"]);
                    model.AddDate = DateTime.Now;
                    OperateContext<tb_Comment>.SetServer.Add(model);

                }
                else
                {
                    jsonM.Status = "err";
                    jsonM.Msg = "用户未登录";
                }
            }
            catch (Exception ex)
            {
                jsonM.Status = "err";
                jsonM.Msg = "修改数据发生错误！ 消息：" + ex.Message;
                LogHelper.WriteLog(typeof(ProJectController), ex);
            }
            return Json(jsonM);
        }
        #endregion

        #region 9、[我等你] 提交订单 + SubOrder()
        /// <summary>
        /// 提交订单
        /// </summary>
        /// <returns></returns>
        public ActionResult SubOrder()
        {
            var ordernum = FytRequest.GetQueryString("o");//订单号
            if (!string.IsNullOrEmpty(ordernum))
            {
                //获取订单信息
                var model = OperateContext<lv_ProjectOrder>.SetServer.GetModel(m => m.Number == ordernum);
                //获取我等你信息
                var project = OperateContext<lv_ProJect>.SetServer.GetModel(m => m.ID == model.ProJectId);
                //判断支付类型
                if (model.PayName == "支付宝")
                {
                    return RedirectToAction("Index", "AliPays",
                new { o = Server.UrlEncode(model.Number), t = Server.UrlEncode(project.Title), re = Server.UrlEncode(project.Title), m = Server.UrlEncode(model.PayPrice.ToString(CultureInfo.InvariantCulture)) });
                }
                else if (model.PayName == "PayPal")
                {
                    return RedirectToAction("SetExpressCheckout", "Paypal",
                 new { i = Server.UrlEncode(model.ID.ToString()), o = Server.UrlEncode(model.Number), t = Server.UrlEncode(project.Title), re = Server.UrlEncode(project.Title), m = Server.UrlEncode(model.PayPrice.ToString(CultureInfo.InvariantCulture)) });
                }
                else if (model.PayName == "信用卡")
                {
                    return RedirectToAction("SetExpressCheckout", "Paypal",
                 new { i = Server.UrlEncode(model.ID.ToString()), o = Server.UrlEncode(model.Number), t = Server.UrlEncode(project.Title), re = Server.UrlEncode(project.Title), m = Server.UrlEncode(model.PayPrice.ToString(CultureInfo.InvariantCulture)) });
                }
            }
            return View();
        }
        #endregion
    }
}