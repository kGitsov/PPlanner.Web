$(function () {
    jQuery.fn.exists = function () { return this.length > 0; }

    var fixHelper = function (e, ui) {
        ui.siblings(":first").children().each(function () {
            $(this).width($(this).width());
        });
        ui.children().each(function () {
            $(this).width($(this).width());
        });
        //return $("<div class='draggedDiv'></div>").text(ui.text());
        return ui;
    };

    if ($("#tsortable").exists())
    $("#tsortable tbody").sortable({
        placeholder: "table-state-highlight",
        opacity: 0.7,
        update: function (event, ui) {

            var position = $("#tsortable tbody tr").index(ui.item);
            var id = $(ui.item).attr("data-id");

            //alert(position + " : " + id);
            $("#StatusMessage").text("Изпратено.");
            var urlStr = $("#PrioritySortURL").val();
            $.post(urlStr, { id: id, position: position })
            .done(function () { $("#StatusMessage").text("Промените са запазени."); })
            .fail(function () { $("#StatusMessage").text("Възникна грешка!"); });

        },
        helper: fixHelper
    });

    if ($(".dropSprint").exists())
    $(".dropSprint").droppable({
        hoverClass: "ui-state-hover",
        tolerance: "pointer",
        drop: function (event, ui) {
            $(this).find(".placeholder").remove();
            
            var prjId = $("#ProjectId").val();
            var ustryid = ui.draggable.attr("data-id");
            var spntid = $(this).attr("data-SprintId");
            //var sprText = $(this).children("a").text();

            //$(this).text( ui.draggable.parent().children().index(ui.draggable)  );

            $("#StatusMessage").text("Изпратено.");

            var urlStr = $("#setSprintURL").val();

            $.post(urlStr, { ProjectId: prjId, SprintId: spntid, UserStoryId: ustryid }, function (res) {
                if (res) {

                    var f = "ul li [data-SprintId=" + spntid + "]";

                    ui.draggable.find("td:nth-child(5)").html(res);

                    $("#StatusMessage").text("Промените са запазени.");
                } else {
                    //ui.draggable.find("td:nth-child(5)").html("Error");

                    $("#StatusMessage").text("Възникна грешка!");
                }
            }).fail(function () {
                $("#StatusArea").text("Възникна грешка!");
            });

        }
    })

    //$("#tsortable tbody").disableSelection();

    if ($("#sprintList").exists())
        $("#sprintList tbody tr").draggable({ revert: true, helper: "clone", opacity: 0.7 });

    if ($(".dropUser").exists())
        $(".dropUser").droppable({
            hoverClass: "ui-state-hover",
            tolerance: "pointer",
            drop: function (event, ui) {
                $(this).find(".placeholder").remove();

                var userstoryId = ui.draggable.attr("data-id");
                var userid = $(this).attr("data-UserId");

                $("#StatusMessage").text("Изпратено.");

                var urlStr = $("#setUserURL").val();
                $.post(urlStr, { UserId: userid, UserStoryId: userstoryId }, function (res) {
                    if (res.length > 0) {

                        //var f = "ul li [data-SprintId=" + spntid + "]";
                        ui.draggable.find("td:nth-child(5)").html(res);

                        $("#StatusMessage").text("Промените са запазени.");
                    } else {
                        //ui.draggable.find("td:nth-child(5)").html("Error");

                        $("#StatusMessage").text("Възникна грешка!");
                    }
                }).fail(function () {
                    $("#StatusArea").text("Възникна грешка!");
                });

            }
        })

    //-------------------------------
    //Common elements

    //$("input.inputDate").datepicker({ dateFormat: "dd/mm/yy" });

    if ($('input[type="datetime"]').exists()) $('input[type="datetime"]').datepicker({ dateFormat: "dd/mm/yy" });

    if ($("form").exists()) $("form").addClass("form-horizontal");

    if ($(".alert").exists()) $(".alert").hide().fadeIn(1000);

    if ($("#StatusArea").exists()) $("#StatusArea").hide();

    $(document).ajaxStart(function () {
        var options = {};
        $("#StatusArea").show("highlight");
    });

    $(document).ajaxStop(function () {
        $("#StatusArea").fadeOut(2000);
    });


    //-------------------------------
    //Graph

    

    //var graphholder = 

    if ($("#graphholder").exists()) {

                var prjId = $("#ProjectId").val();
                $("#StatusMessage").text("Графиката се актуализира");

                var maxx = 13.5; //total days
                var maxy = 12.5; //total effort

                var d1 = [[0, maxy], [maxx, 0]];
                //total effort of work completed each day. [day,effort]
                var d2 = [[0, 12.5], [1, 12], [2, 11.5], [3, 9]];

                var urlStr = $("#GraphPostUrl").val();

                $.post(urlStr, { ProjectId: prjId }, function (data) {

                if (data) {
                    $("#StatusMessage").text("Графиката е актуализирана");
                    var gdata;

                    if (JSON.stringify) {
                            gdata = jQuery.parseJSON(JSON.stringify(data));
                    } else {
                        gdata = jQuery.parseJSON(String(data));
                    }

                    if (gdata) {
                        maxx = gdata.maxx;
                        maxy = gdata.maxy;

                        d2 = gdata.d2;
                        d1 = [[0, maxy], [maxx, 0]];

                        DrawGraph(gdata);
                    }

                } else {
                    $("#StatusMessage").text("Възникна грешка! Прекъсна връзката, докато се актуализира графиката.");
                }

            }).fail(function () {
                $("#StatusArea").text("Възникна грешка! Прекъсна връзката, докато се актуализира графиката.");
            });

        function DrawGraph(gdata) {
            $.plot("#graphholder", [{
                data: d1,
                lines: { show: true },
                color: "#000",
                shadowSize: 0,
                label: "Ideal project"

            }, {
                data: d2,
                lines: { show: true, fill: true },
                color: "#2e89cb",
                label: "Project"

            }], {
                xaxis: {
                min: 0,
                max: gdata.maxx
            },
                yaxis: {
                min: 0,
                max: gdata.maxy
            }
            });
        }

    }

    //-------------------------------
    //Graph2



    //var graphholder = 

    if ($("#graphholder2").exists()) {

        var prjId = $("#ProjectId").val();
        $("#StatusMessage").text("Графиката се актуализира");

        var maxx = 13.5; //total days
        var maxy = 12.5; //total effort

        var d2;

        var urlStr = $("#GraphPostUrl2").val();

        $.post(urlStr, { ProjectId: prjId }, function (data) {

            if (data) {
                $("#StatusMessage").text("Графиката е актуализирана");
                var gdata;

                if (JSON.stringify) {
                    gdata = jQuery.parseJSON(JSON.stringify(data));
                } else {
                    gdata = jQuery.parseJSON(String(data));
                }

                if (gdata) {
                    maxx = gdata.maxx;
                    maxy = gdata.maxy;

                    d2 = gdata.u2;
                    d3 = gdata.p2;
                    d1 = [[0, maxy], [maxx, 0]];

                    DrawGraph2(gdata);
                }

            } else {
                $("#StatusMessage").text("Възникна грешка! Прекъсна връзката, докато се актуализира графиката.");
            }

        }).fail(function () {
            $("#StatusArea").text("Възникна грешка! Прекъсна връзката, докато се актуализира графиката.");
        });

        function DrawGraph2(gdata) {
            $.plot("#graphholder2", [{
                data: d2,
                lines: { show: true, fill: true },
                points: { show: true, radius: 3 },
                color: "#2e89cb",
                label: "User score"
            }, {
                data: d3,
                lines: { show: true, fill: true },
                points: { show: true, radius: 3 },
                color: "#ff7f7f",
                label: "PM score"
            }], {
                xaxis: {
                    min: 0,
                    max: gdata.maxx,
                    ticks: gdata.maxx
                },
                yaxis: {
                    min: 0,
                    max: gdata.maxy,
                    ticks: 11
                }
            });
        }
    }

    if ($("#graphholder3").exists()) {

        var prjId = $("#ProjectId").val();
        $("#StatusMessage").text("Графиката се актуализира");

        var maxx = 13.5; //total days
        var maxy; //total effort
        var miny;
        var d2;

        var urlStr = $("#GraphPostUrl3").val();

        $.post(urlStr, { ProjectId: prjId }, function (data) {

            if (data) {
                $("#StatusMessage").text("Графиката е актуализирана");
                var gdata;

                if (JSON.stringify) {
                    gdata = jQuery.parseJSON(JSON.stringify(data));
                } else {
                    gdata = jQuery.parseJSON(String(data));
                }

                if (gdata) {
                    maxx = gdata.maxx;
                    maxy = gdata.maxy;
                    miny = gdata.miny;

                    d3 = gdata.p2;
                    d1 = [[miny, maxy], [maxx, 0]];
                    d2 = [[0, 0], [maxx, 0]];

                    DrawGraph3(gdata);
                }

            } else {
                $("#StatusMessage").text("Възникна грешка! Прекъсна връзката, докато се актуализира графиката.");
            }

        }).fail(function () {
            $("#StatusArea").text("Възникна грешка! Прекъсна връзката, докато се актуализира графиката.");
        });

        function DrawGraph3(gdata) {               
            //var xticks = [];
            //for (var i = 0; i < gdata.maxx; i++) {
                //xticks.push([i, gdata.ids[i]])
            //}
            $.plot("#graphholder3", [{                
                data: d3,
                threshold: {
                    below: 0,
                    color: "green"
                },
                    lines: { show: true, fill: true },                
                    color: "#ff7f7f",
                    label: "Good"
                
            },{
                data: d2,
                threshold: {
                    below: 2,
                    color: "green"
                },
                lines: { show: true, fill: true },
                points: { show: true, radius: 3 },
                color: "green",
                label: "Difference"
            }]
            , {
                xaxis: {
                    min: 0,
                    max: gdata.maxx,
                    ticks: gdata.maxx//xticks
                },
                yaxis: {
                    min: gdata.miny-1,
                    max: gdata.maxy+1,
                    ticks: Math.sqrt(Math.pow(gdata.miny,2)+Math.pow(gdata.maxy,2))+1
                    
                }
            
            });


        }
    }



    if ($("#graphholder4").exists()) {

        var prjId = $("#ProjectId").val();
        $("#StatusMessage").text("Графиката се актуализира");

        var maxx = 13.5; //total days
        var maxy = 12.5; //total effort

        var d2;

        var urlStr = $("#GraphPostUrl4").val();

        $.post(urlStr, { ProjectId: prjId }, function (data) {

            if (data) {
                $("#StatusMessage").text("Графиката е актуализирана");
                var gdata;

                if (JSON.stringify) {
                    gdata = jQuery.parseJSON(JSON.stringify(data));
                } else {
                    gdata = jQuery.parseJSON(String(data));
                }

                if (gdata) {
                    maxx = gdata.maxx;
                    maxy = gdata.maxy;

                    d2 = gdata.z2;
                    d3 = gdata.p2;
                    d1 = [[0, maxy], [maxx, 0]];

                    DrawGraph4(gdata);
                }

            } else {
                $("#StatusMessage").text("Възникна грешка! Прекъсна връзката, докато се актуализира графиката.");
            }

        }).fail(function () {
            $("#StatusArea").text("Възникна грешка! Прекъсна връзката, докато се актуализира графиката.");
        });

        function DrawGraph4(gdata) {
            $.plot("#graphholder4", [{
                data: d2,
                lines: { show: true, fill: true },
                points: { show: true, radius: 3 },
                color: "#2e89cb",
                label: "Manager score"
            }, {
                data: d3,
                lines: { show: true, fill: true },
                points: { show: true, radius: 3 },
                color: "#ff7f7f",
                label: "User score"
            }], {
                xaxis: {
                    min: 0,
                    max: gdata.maxx,
                    ticks: gdata.maxx
                },
                yaxis: {
                    min: 0,
                    max: gdata.maxy,
                    ticks: gdata.maxy
                }
            });
        }
    }



    
    //----------------------------------
    //Sprint board

    if ($("#ToDoDrop").exists()) {

        $(".dropList li").draggable({
            cancel: "a.ui-icon", // clicking an icon won't initiate dragging
            revert: "invalid", // when not dropped, the item will revert back to its initial position
            containment: "document",
            helper: "clone",
            cursor: "move"
        });


        $("#ToDoDrop").droppable({
            accept: ".dropList li",
            drop: function (event, ui) {
                var $ToDoDrop = $("#ToDoDrop");
                var $item = ui.draggable;
                UpdateStory($ToDoDrop, $item);
            }
        });

        $("#progressDrop").droppable({
            accept: ".dropList li",
            drop: function (event, ui) {
                var $progressDrop = $("#progressDrop");
                var $item = ui.draggable;
                UpdateStory($progressDrop, $item);
            }
        });

        $("#doneDrop").droppable({
            accept: ".dropList li",
            drop: function (event, ui) {
                var $doneDrop = $("#doneDrop");
                var $item = ui.draggable;
                UpdateStory($doneDrop, $item);
            }
        });

        function UpdateStory($target, $item) {
            var statusid = $target.attr("data-statusid");
            var ustoryid = $item.attr("data-userstoryid");

            //$item.text(statusid + " " + ustoryid);

            $("#StatusMessage").text("Изпращане на промените");

            var urlStr = $("#SetStatusUrl").val();
            $.post(urlStr, { UserStoryId: ustoryid, StatusId: statusid }, function (data) {

                if (data) {
                    $("#StatusMessage").text("Записано");

                    $item.fadeOut('fast', function () {
                        $item.appendTo($target).fadeIn('fast');
                    });

                } else {
                    $("#StatusMessage").text("Възникна грешка!");
                }

            }).fail(function () {
                $("#StatusArea").text("Възникна грешка!");
            });

        }

    }


});


/*

    $("#new-project-form").dialog({
        autoOpen: false,
        height: 420,
        width: 450,
        modal: true,
        buttons: {
            Create: function () {
                var bValid = true;
                if (bValid) {
                    $(this).dialog("close");
                }
            },
            Cancel: function () {
                $(this).dialog("close");
            }
        },
        close: function () {
            //allFields.val("").removeClass("ui-state-error");
        }
    });


    $("#create-project")
        .button()
        .click(function () {
            $("#new-project-form").dialog("open");
        });

    //-----------------------backlog forms
    $("#UserStoryEditForm").dialog({
        autoOpen: false,
        height: 400,
        width: 450,
        modal: true,
        buttons: {
            Create: function () {
                var bValid = true;
                if (bValid) {
                    $(this).dialog("close");
                }
            },
            Cancel: function () {
                $(this).dialog("close");
            }
        },
        close: function () {
            //allFields.val("").removeClass("ui-state-error");
        }
    });


    $("#tsortable a.editForm")
        .button()
        .click(function () {
            $("#UserStoryEditForm").dialog("open");
        });

    $("#sprintList a.editForm")
    .button()
    .click(function () {
        $("#UserStoryEditForm").dialog("open");
    });



    $("#sprint-Form").dialog({
        autoOpen: false,
        height: 350,
        width: 400,
        modal: true,
        buttons: {
            Create: function () {
                var bValid = true;
                if (bValid) {
                    $(this).dialog("close");
                }
            },
            Cancel: function () {
                $(this).dialog("close");
            }
        },
        close: function () {
            //allFields.val("").removeClass("ui-state-error");
        }
    });


    $("a.sprintForm")
        .button()
        .click(function () {
            $("#sprint-Form").dialog("open");
        });

*/