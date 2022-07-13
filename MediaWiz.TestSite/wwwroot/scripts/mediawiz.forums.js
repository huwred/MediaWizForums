        var MediaWiz = MediaWiz ||
        {
            tools: "code undo redo | styleselect | bullist numlist | indent outdent | link codesample emoticons",
            returnUrl: "",
            deletePost : function(postId) {
                if (window.confirm("Are you sure you want to delete this post?")) {
                    var del = $.get("/deletepost/" + postId);
                    del.done(function (data, status) {
                        location.reload();
                    });
                    del.fail(function (result) { alert("sorry, there was an error deleting this post"); });
                }
            },
            editPost: function(postId) {
                $.ajax({
                    type: "GET",
                    url: "/umbraco/surface/forumssurface/editPost/" + postId,
                    // other settings
                    success: function (result) {
                        $("#partial-form").html(result);

                    },
                    error: function(error) {
                        console.log(error);
                        alert(error);
                    }
                });
            },
            lockPost: function(postId) {
                if (window.confirm("Are you sure you want to lock/unlock this post?")) {
                    var locking = $.get("/lockpost/" + postId);
                    locking.done(function(data, status) {
                        location.reload();
                    });
                    locking.fail(function (result) { alert("sorry, there was an error locking this post" + result); });
                }
            },
            lockUser: function(user,mode) {
                $.ajax({
                    type: "GET",
                    url: "/lockuser/" + user + "/?mode=" + mode,
                    success: function (result) {
                        if (result) {
                            location.reload(true);
                        }
                    },
                    error: function(error) {
                        alert(error);
                    }
                });
            },
            captchaCheck: function(answer) {
                $.ajax({
                    url: '/captchacheck/' + answer,
                    type: 'GET',
                    success: function(data) {
                        
                        if (data) {
                            $("#captcha-check").hide();
                            $('#captcha-form-id').show();
                            $("#captcha-form-id").clearValidation();

                        } else {
                            $("#Captcha").val("");
                            $("#Captcha").attr("placeholder", "Incorrect, please try again.");
                        }
                    },
                    error: function(jqXHR, exception) {
                        console.log(jqXHR);
                        return false;
                    }
                });
            },
            InitTinyMce:  function (selector) {
                window.tinymce.init({
                    selector: selector,
                    browser_spellcheck: true,
                    contextmenu: false,
                    plugins: "link lists anchor codesample image code emoticons",
                    /*content_css: "writer",*/
                    toolbar: MediaWiz.tools,
                    file_picker_types: "image",
                    images_upload_url: "/forumupload",
                    images_reuse_filename: true,
                    statusbar: false,
                    menubar: false,
                    relative_urls: false,
                    remove_script_host: false,
                    convert_urls: true,
                    init_instance_callback: function(editor) {
                        editor.on("OpenWindow",
                            function(e) {
                                $('[role=tab]:contains("General")').hide();
                            });
                    }
                });
                document.addEventListener("focusin",
                    (e) => {
                        if (e.target.closest(".tox-tinymce-aux, .moxman-window, .tam-assetmanager-root") !== null) {
                            e.stopImmediatePropagation();
                        }
                    });
            }
        };

        $(document).ready(function() {

            if (MediaWiz.returnUrl.length > 1) {
                window.pageRedirect(MediaWiz.returnUrl);
            }

            $(".btn-cancel").on("click", function(e) {
                history.back();
            });

            $(".post-delete").click(function(e) {
                e.stopPropagation();
                e.preventDefault();
                MediaWiz.deletePost($(this).data("postid"));
            });

            $(".post-lock").on("click",function (e) {
                e.stopPropagation();
                e.preventDefault();
                MediaWiz.lockPost($(this).data("postid"));
            });

            $(".post-edit").on("click",function (e) {
                e.stopPropagation();
                e.preventDefault();
                MediaWiz.editPost($(this).data("postid"));
            });

            $(".lock-user").on("click",function(e) {
                e.stopPropagation();
                e.preventDefault();
                MediaWiz.lockUser($(this).data("userid"), $(this).data("mode"));
            });

            $("#editPostModal").on("show.bs.modal",function() {
                setTimeout(function() {
                    MediaWiz.InitTinyMce("textarea.edit-body");
                }, 300);
            });

            $("#editPostModal").on("hide.bs.modal", function () {
                tinymce.remove("#partial-form textarea");
            });
        });
