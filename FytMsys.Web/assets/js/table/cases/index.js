﻿$(function () {
    tb.initTable(1);
    //检索/刷新
    $("#btnKey,#tb-refresh").click(function () {
        tb.initTable(1);
    });
    //页码调整
    $("#sel_page").change(function () {
        tb.initTable(1);
    });
    $("#Modfiy").click(function () {
        var vals = '', lsum = 0;
        $('input[name="checkbox_name"]:checked').each(function () {
            vals = $(this).val();
            lsum += 1;
        });
        if (!vals) { dig.alertError("提示", "请选择要编辑的信息！"); return; }
        if (lsum > 1) { dig.alertError("提示", "编辑信息只能选择一项！"); return; }
        dig.addModel("案例所有信息", url + "CaseIndex/?caseId=" + vals, 1000, 550, function () {
            //tb.initTable(1);
        });
    });
});

var url = "/FytAdmin/Cases/", collSum = 9, tbody = $("#tables>tbody"), tdata = $("#tlist"), tb = {
    initTable: function (pageIndex) {
        var pageSize = $("#sel_page").val(), maxi = 0;
        $.ajax({
            type: "post",
            url: url + "IndexData",
            beforeSend: function () { tbody.html(tb.ajaxLoading()); },
            data: {
                pageSize: pageSize, pageIndex: pageIndex, key: escape($("#txtKey").val()),
                audit: $("#audit").val(), beginTime: $("#beginTime").val(), endTime: $("#endTime").val(),
                ClassId: $("#ClassId").val()
            },
            success: function (res) {
                if (res.Status == "y") {
                    if (res.Data == "" || res.Data == null) {
                        tbody.empty().html(tb.ajaxNull());
                    } else {
                        tbody.empty();
                        tdata.tmpl(res.Data).appendTo('#trows');
                        fyt.asyncall();
                        //使用col插件实现表格头宽度拖拽
                        $(".table").colResizable();
                        //alert(res.PageRows + "---" + res.PageTotal);
                        var endPage = (pageIndex * pageSize > res.PageRows ? res.PageRows : pageIndex * pageSize);
                        $("#tables_info").html('显示第 ' + ((pageIndex - 1) * pageSize + 1) + ' 至 ' + endPage + ' 项结果，共 ' + res.PageRows + ' 项');
                        $("ul.pagination").html(MvcPage(res.PageTotal, pageIndex, "initTable", "tb"));
                        //双击行事件
                        $("#tables tr").dblclick(function () {
                            var eId = $(this).find("input[type='checkbox']").val();
                            dig.addModel("案例所有信息", url + "CaseIndex/?caseId=" + eId, 1000, 550, function () {
                                //tb.initTable(1);
                            });
                        });
                    }
                } else {
                    dig.alertError("错误提示", res.Msg);
                }
            },
            error: function (e) {
                console.log(e);
            }
        });
    },
    ajaxLoading: function () {
        return "<tr><td colspan=\"" + collSum + "\" style='text-align:center;'><img src='/assets/images/skin/T1BHtLFbFdXXaHNz_X-16-16.gif' /> loading...</td></tr>";
    },
    ajaxNull: function () {
        return "<tr><td colspan=\"" + collSum + "\" style='text-align:center;'><img src='/assets/images/admin/face-sad.png' /> <span>　暂无数据</span></td></tr>";
    },
    deletes: function (id) {
        var vals = '';
        if (id == 0) {
            $('input[name="checkbox_name"]:checked').each(function () {
                vals += $(this).val() + ',';
            });
            if (!vals) {
                dig.alertError("提示", "对不起，请选中您要操作的记录！");
                return;
            }
        } else {
            vals = id;
        }
        var msg = "删除记录后不可恢复，您确定吗？";
        dig.confim("删除确认", msg, function () {
            $.post(url + "DownLoadDeleteBy", { id: vals }, function (res) {
                if (res.Status == "y") tb.initTable(1);
                else dig.alertError("消息", res.Msg);
                $(":checkbox").attr("checked", false);
            }, "json");
        });
    }
};

function ResStatus(s) {
    s = parseInt(s);
    switch (s) {
        case 1:
            return '<span class="text-warning"><i class="im-checkmark4"></i> 审核中</span>';
        case 2:
            return '<span class="text-warning"><i class="im-minus"></i> 处理中</span>';
        case 3:
            return '<span class="text-navy"><i class="im-checkmark3"></i> 已完成</span>';
    }
}

function GetImg(m) {
    if (!m) {
        return '<span title="图片" class="text-navy"><i class="im-googleplus3"></i></span>';
    }
    return m;
}