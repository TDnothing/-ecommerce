﻿$(function () {

    $("#updoc").click(function () {
        $("#fileUrls").unbind();
        $("#fileUrls").click(function () {
           
        });
        
        $("#fileUrls").change(function () {
            upDoc();
        });
    });

});
var upDoc = function () {
    var subUrl = "/Account/UpAuthDocs/?upFiles=fileUrls&isImg=1&isThum=0&isWater=0";
    $("#forms").ajaxSubmit({
        beforeSubmit: function () {
            //$(".sign-up").attr("disabled", "disabled").html(" 正在上传...");
        },
        success: function (data) {
            if (data.Status == "y") {
                $("#img").html('<img  src="' + data.Data + '" width="400" height="260" />');
                swal('消息', '图片上传成功，请等待审核！', 'success');
            } else {
                swal('消息', data.Msg, 'error');
            }
            //$(".sign-up").attr("disabled", false).html(" 单文件上传");
            $("#fileUrls").unbind();
        },
        error: function (e) {
            //$(".sign-up").attr("disabled", false).html(" 单文件上传");
            console.log(e);
        },
        url: subUrl,
        type: "post",
        dataType: "json",
        timeout: 600000
    });
};

