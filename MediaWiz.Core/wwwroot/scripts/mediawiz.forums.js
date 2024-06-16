        var MediaWiz = MediaWiz ||
        {
            tools: "code undo redo | styleselect | bullist numlist | indent outdent | link codesample",
            returnUrl: "",
            currLang: getLang(),
            editPost: function(postId) {
                $.ajax({
                    type: "GET",
                    url: "/umbraco/surface/forumssurface/editPost/" + postId,
                    success: function (result) {
                        $("#partial-form").html(result);

                    },
                    error: function(error) {
                        console.log(error);
                        alert(error);
                    }
                });
            },
            deletePost : function(postId) {
                if (window.confirm("Are you sure you want to delete this post?")) {
                    var del = $.get("/deletepost/" + postId);
                    del.done(function (data, status) {
                        location.reload();
                    });
                    del.fail(function (result) { alert("sorry, there was an error deleting this post"); });
                }
            },
            markAnswer: function(postId) {
                if (window.confirm("Are you sure you want to mark this post as the answer?")) {
                    var locking = $.get("/markanswer/" + postId);
                    locking.done(function(data, status) {
                        location.reload();
                    });
                    locking.fail(function (result) { alert("sorry, there was an error marking this post as the answer" + result); });
                }
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
            approvePost: function(postId) {
                if (window.confirm("Are you sure you want to Approve this post?")) {
                    var locking = $.get("/approve/" + postId);
                    locking.done(function(data, status) {
                        location.reload();
                    });
                    locking.fail(function (result) { alert("sorry, there was an error approving the post" + result); });
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
            captchaCheck: function(answer, callback) {
                $.ajax({
                    url: '/captchacheck/' + answer,
                    type: 'GET',
                    success: function(data) {

                        if (callback) {callback(data); }

                    },
                    error: function(jqXHR, exception) {
                        alert(jqXHR);
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
                    toolbar: MediaWiz.tools,
                    file_picker_types: "image",
                    images_upload_url: "/forumupload",
                    images_reuse_filename: true,
                    statusbar: false,
                    menubar: false,
                    relative_urls : false,
                    remove_script_host : true,
                    document_base_url : window.location.protocol + "//" + window.location.host + "/",
                    convert_urls: true,
                    init_instance_callback: function(editor) {
                        //editor.on("OpenWindow",
                        //    function(e) {
                        //        $('[role=tab]:contains("General")').removeClass("mce-active");
                        //        $('#' + $('[role=tab]:contains("General")').attr("aria-controls")).hide();
                        //        $('[role=tab]:contains("Upload")').addClass("mce-active");
                        //        $('#' + $('[role=tab]:contains("Upload")').attr("aria-controls")).show();
                        //    });
                    },
                    content_css: false,
                    content_style: `

                        blockquote {
                            border-left: 4px solid #ddd;
                            padding: 0 15px;
                            color: #777;
                        }
                        .mce-content-body img {
                            max-width: 99% !important;
                            height: auto;
                        } 
                    `
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
            $(".post-quote").on("click", function(e) {
                e.stopPropagation();
                e.preventDefault();

                tinymce.activeEditor.setContent("<blockquote>" + $("#postcontent_" + $(this).data("postid")).html() + "</blockquote><br/> ");
                goToTheEnd();
            });
            $(".post-delete").on("click", function(e) {
                e.stopPropagation();
                e.preventDefault();
                MediaWiz.deletePost($(this).data("postid"));
            });

            $(".post-lock").on("click",function (e) {
                e.stopPropagation();
                e.preventDefault();
                MediaWiz.lockPost($(this).data("postid"));
            });
            $(".post-approve").on("click",function (e) {
                e.stopPropagation();
                e.preventDefault();
                MediaWiz.approvePost($(this).data("postid"));
            });
            $(".post-answer").on("click",function (e) {

                e.stopPropagation();
                e.preventDefault();
                MediaWiz.markAnswer($(this).data("postid"));
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
                    MediaWiz.InitTinyMce("#partial-form textarea");
                }, 300);
            });

            $("#editPostModal").on("hide.bs.modal", function () {
                tinymce.remove("#partial-form textarea");
            });
        });
        function getLang() {
            if (navigator.languages != undefined) 
                return navigator.languages[0]; 
            return navigator.language;
        }
        function goToTheEnd() {
            var ed=tinyMCE.activeEditor;
            var root=ed.dom.getRoot();  // This gets the root node of the editor window
            var lastnode=root.childNodes[root.childNodes.length-1]; // And this gets the last node inside of it, so the last <p>...</p> tag
            if (tinymce.isGecko) {
                // But firefox places the selection outside of that tag, so we need to go one level deeper:
                lastnode=lastnode.childNodes[lastnode.childNodes.length-1];
            }
            // Now, we select the node
            ed.selection.select(lastnode);
            // And collapse the selection to the end to put the caret there:
            ed.selection.collapse(false);
        }
        $( "li.reply" ).hover(
            function() {
                $(this).find(".tool-label").show();
            }, function() {
                $(this).find(".tool-label").hide();
            }
        );
        $( "li.topic" ).hover(
            function() {
                $(this).find(".tool-label").show();
            }, function() {
                $(this).find(".tool-label").hide();
            }
        );