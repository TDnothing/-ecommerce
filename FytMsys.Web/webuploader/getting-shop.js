﻿// 图片上传demo
jQuery(function () {
    var $ = jQuery,
        $list = $('#fileList'),
        // 优化retina, 在retina下这个值是2
        ratio = window.devicePixelRatio || 1,

        // 缩略图大小
        thumbnailWidth = 155 * ratio,
        thumbnailHeight = 120 * ratio,

        // Web Uploader实例
        uploader;

    // 初始化Web Uploader
    uploader = WebUploader.create({

        // 自动上传。
        auto: true,

        // swf文件路径
        swf: '/webuploader/Uploader.swf',

        // 文件接收服务端。
        server: '/FytAdmin/WebUpload/FileUpload',

        // 选择文件的按钮。可选。
        // 内部根据当前运行是创建，可能是input元素，也可能是flash.
        pick: '#filePicker',
        //限制最多上传5张
        fileNumLimit:5,
        // 只允许选择文件，可选。
        accept: {
            title: 'Images',
            extensions: 'gif,jpg,jpeg,bmp,png',
            mimeTypes: 'image/*'
        }
    });
    var indexs = 10;
    indexs = indexs - $("#fileList>.file-item").length;
    // 当有文件添加进来的时候
    uploader.on('fileQueued', function (file) {
        indexs--;
        var $li = $(
                '<div id="' + file.id + '" class="file-item thumbnail">' +
                    '<img>' +
                    '<div class="info">' + file.name + '</div>' +
                    '<div><input type="radio" name="file_summary" ' + (indexs == 9 ? 'checked="checked"' : "") + ' class="input-sm form-control" style="float:left;margin-top:9px;" value="' + indexs + '" /><span style="display:block; float:left;width:95px; margin-top:8px;">设置为主图</span><input type="text" name="file_sort_' + indexs + '" class="input-sm form-control" style="width:26px; text-align:center; float:left; margin-left:-1px; padding:0px 3px;" value="' + indexs + '" data-placement="top" data-toggle="tooltip" title="排序，数字越大越靠前" /></div>' +
                    '<a href="javascript:void(0)" data-src="" data-file-id="' + file.id + '" onclick="delFile(' + indexs + ',\'' + file.id + '\')" data-id="' + indexs + '" class="pldel">×</a>' +
                    '<input type="hidden" name="file_name_' + indexs + '" value="" />' +
                '</div>'
                ),
            $img = $li.find('img');

        $list.append($li);

        // 创建缩略图
        uploader.makeThumb(file, function (error, src) {
            if (error) {
                $img.replaceWith('<span>不能预览</span>');
                return;
            }

            $img.attr('src', src);
        }, thumbnailWidth, thumbnailHeight);
    });

    // 文件上传过程中创建进度条实时显示。
    uploader.on('uploadProgress', function (file, percentage) {
        var $li = $('#' + file.id),
            $percent = $li.find('.progress span');

        // 避免重复创建
        if (!$percent.length) {
            $percent = $('<p class="progress"><span></span></p>')
                    .appendTo($li)
                    .find('span');
        }

        $percent.css('width', percentage * 100 + '%');
    });

    // 文件上传成功，给item添加成功class, 用样式标记上传成功。
    uploader.on('uploadSuccess', function (file, res) {
        $('#' + file.id).addClass('upload-state-done');
        if (res.Status == "y") {
            $("#imlist").val($("#imlist").val() + $('#' + file.id).find(".pldel").attr("data-id") + "|");
            $('#' + file.id).find("input[type='hidden']").val(res.Data);
        }
    });

    // 文件上传失败，现实上传出错。
    uploader.on('uploadError', function (file) {
        var $li = $('#' + file.id),
            $error = $li.find('div.error');
        // 避免重复创建
        if (!$error.length) {
            $error = $('<div class="error"></div>').appendTo($li);
        }
        $error.text('上传失败-5秒消失');
        setTimeout(function () {
            $('#' + file.id).remove();
        }, 4000);
    });

    // 完成上传完了，成功或者失败，先删除进度条。
    uploader.on('uploadComplete', function (file) {
        $('#' + file.id).find('.progress').remove();
    });
});

function delFile(t, fw) {
    $("#" + fw).remove();
    var th = $("#imlist").val();
    if (th.indexOf(t + '|') > 0) {
        var v = th.replace(t + '|', "");
        $("#imlist").val(v);
    }
}