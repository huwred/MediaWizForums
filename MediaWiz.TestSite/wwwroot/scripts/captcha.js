
        $(document).on("click","#btn-captcha", function(e) {
            e.preventDefault();
            MediaWiz.captchaCheck($("#Captcha").val(), function(data) {
                alert(data);
                if (data) {
                    $("#captcha-check").hide();
                    $("#captcha-refresh").hide();
                    $("#form-submit").show();
                    $("#form-submit-label").show();
                    $("#captcha-form-id").clearValidation();
                    $("#captcha-form-id").show();
                } else {
                    $("#Captcha").val("");
                    $("#Captcha").attr("placeholder", "Incorrect, please try again.");
                }
            });
        });
        $(document).on("click","#captcha-refresh", function(e) {
            alert("refresh");
            e.preventDefault();
            $.ajax({
                url: "/umbraco/surface/forumssurface/refreshcaptcha",
                success: function (data) {
                    $("#captchaimage-div").html(data);
                }
            });
        });
