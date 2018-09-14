﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entity
{
    /// <summary>
    /// 提现申请
    /// </summary>
    public class lv_GetMoney
    {
        /// <summary>
        /// 自动增长
        /// </summary>
        [Key]
        public int ID { get; set; }

        /// <summary>
        /// 订单编号
        /// </summary>
        [MaxLength(50)]
        [Display(Name = "编号")]
        public string Number { get; set; }

        /// <summary>
        /// 报名人ID
        /// </summary>
        [Display(Name = "报名人")]
        public int UserId { get; set; }

        /// <summary>
        /// 提现金额
        /// </summary>
        [Display(Name = "提现金额")]
        public decimal Price { get; set; }

        /// <summary>
        /// 提现账号
        /// </summary>
        [MaxLength(50)]
        [Display(Name = "提现账号")]
        public string Account { get; set; }

        /// <summary>
        /// 银行ID
        /// </summary>
        [Display(Name = "银行ID")]
        public int BankId { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        [Display(Name = "状态")]
        public bool Status { get; set; }

        /// <summary>
        /// 添加时间
        /// </summary>
        [Display(Name = "添加时间")]
        public DateTime AddTime { get; set; }

        /// <summary>
        /// 区号
        /// </summary>
        [Display(Name = "区号")]
        public string AreaNum { get; set; }

        /// <summary>
        /// 关联到用户表
        /// </summary>
        [ForeignKey("UserId")]
        public virtual tb_User tb_User { get; set; }
    }
}
