﻿function initSearch() {
    //账册号
    var store_recordid = Ext.create('Ext.data.JsonStore', {
        fields: ['CODE', 'NAME'],
        data: common_data_recordid
    });
    var s_combo_recordid = Ext.create('Ext.form.field.ComboBox', {
        id: 's_combo_recordid',
        editable: false,
        store: store_recordid,
        fieldLabel: '账册号',
        labelWidth: 50,
        width: 220,
        displayField: 'NAME',
        name: 'recordid',
        valueField: 'CODE',
        triggerAction: 'all',
        queryMode: 'local'
    });
    //项号
    var field_ITEMNO = Ext.create('Ext.form.field.Text', {
        id: 'field_ITEMNO',
        fieldLabel: '项号',
        labelWidth: 50,
        name: 'ITEMNO',
        flex: .2
    });
    //HS编码
    var field_HSCODE = Ext.create('Ext.form.field.Text', {
        id: 'field_HSCODE',
        fieldLabel: 'HS编码',
        labelWidth: 50,
        name: 'HSCODE'
    });
    //变动状态
    var store_optionstatus = Ext.create('Ext.data.JsonStore', {
        fields: ['CODE', 'NAME'],
        data: optionstatus_js_data
    });
    var s_combo_optionstatus = Ext.create('Ext.form.field.ComboBox', {
        id: 's_combo_optionstatus',
        editable: false,
        store: store_optionstatus,
        fieldLabel: '变动状态',
        labelWidth: 60,
        displayField: 'NAME',
        name: 'OPTIONS',
        valueField: 'CODE',
        triggerAction: 'all',
        queryMode: 'local'
    });
    //申请状态
    var store_status = Ext.create('Ext.data.JsonStore', {
        fields: ['CODE', 'NAME'],
        data: status_js_data
    });
    var s_combo_status = Ext.create('Ext.form.field.ComboBox', {
        id: 's_combo_status',
        editable: false,
        store: store_status,
        fieldLabel: '申请状态',
        labelWidth: 60,
        displayField: 'NAME',
        name: 'STATUS',
        valueField: 'CODE',
        triggerAction: 'all',
        queryMode: 'local'
    });
    //非法码比对
    var chk_error = Ext.create('Ext.form.field.Checkbox', {
        id: 'chk_error',
        boxLabel: '非法码比对',
        name: 'error',
        minWidth: 80
    });
    var formpanel = Ext.create('Ext.form.Panel', {
        id: 'formpanel',
        renderTo: 'div_form',
        fieldDefaults: {
            margin: '5'
        },
        items: [
        { layout: 'column', border: 0, margin: '5 0 0 0', items: [s_combo_recordid, field_ITEMNO, field_HSCODE, s_combo_optionstatus, s_combo_status, chk_error] }
        ]
    });
}

function itemsbind() {
    Ext.regModel('RecrodDetail', {
        fields: ['ID', 'OPTIONS', 'STATUS', 'RECORDINFOID', 'CODE', 'ITEMNO', 'HSCODE', 'ADDITIONALNO', 'ITEMNOATTRIBUTE', 'COMMODITYNAME'
            , 'SPECIFICATIONSMODEL', 'UNIT', 'CUSTOMERCODE', 'CUSTOMERNAME', 'REMARK']
    });

    var store_RecrodDetail_lj = Ext.create('Ext.data.JsonStore', {
        model: 'RecrodDetail',
        pageSize: 20,
        proxy: {
            type: 'ajax',
            url: '/RecordInfor/loadRecordDetail',
            reader: {
                root: 'rows',
                type: 'json',
                totalProperty: 'total'
            }
        },
        autoLoad: true,
        listeners: {
            beforeload: function () {
                store_RecrodDetail_lj.getProxy().extraParams = {
                    ITEMNOATTRIBUTE: '料件',
                    RECORDINFORID: Ext.getCmp('s_combo_recordid').getValue(), ITEMNO: Ext.getCmp("field_ITEMNO").getValue(), HSCODE: Ext.getCmp('field_HSCODE').getValue(),
                    OPTIONS: Ext.getCmp('s_combo_optionstatus').getValue(), STATUS: Ext.getCmp("s_combo_status").getValue(),
                    ERROR: Ext.getCmp('chk_error').getValue() == true ? "1" : "0"
                }
            },
            load: function () {
                var total_lj = store_RecrodDetail_lj.getProxy().getReader().rawData.total;
                Ext.getCmp("tabpanel").items.items[0].setTitle("料件(" + total_lj + ")");
            }
        }
    });

    var pgbar_lj = Ext.create('Ext.toolbar.Paging', {
        id: 'pgbar_lj',
        displayMsg: '显示 {0} - {1} 条,共计 {2} 条',
        store: store_RecrodDetail_lj,
        displayInfo: true
    })
    gridpanel_lj = Ext.create('Ext.grid.Panel', {
        id: "gridpanel_lj",
        store: store_RecrodDetail_lj,
        height: 500,
        selModel: { selType: 'checkboxmodel' },
        bbar: pgbar_lj,
        enableColumnHide: false,
        columns: [
        { xtype: 'rownumberer', width: 35 },
        { header: 'ID', dataIndex: 'ID', hidden: true },
        //{ header: '变动状态', dataIndex: 'OPTIONS', width: 110 },
        //{ header: '申请状态', dataIndex: 'STATUS', width: 110 },
        { header: '账册号', dataIndex: 'CODE', width: 130 },
        { header: '项号', dataIndex: 'ITEMNO', width: 80 },
        { header: 'HS编码', dataIndex: 'HSCODE', width: 130 },
        { header: '附加码', dataIndex: 'ADDITIONALNO', width: 80 },
        { header: '项号属性', dataIndex: 'ITEMNOATTRIBUTE', width: 80 },
        { header: '商品名称', dataIndex: 'COMMODITYNAME', width: 200 },
        { header: '规格型号', dataIndex: 'SPECIFICATIONSMODEL', width: 200 },
        { header: '成交单位', dataIndex: 'UNIT', width: 80, renderer: renderOrder },
        //{ header: '报关行', dataIndex: 'CUSTOMERNAME', width: 150 },
        { header: '备注', dataIndex: 'REMARK', width: 150 }
        ],
        viewConfig: {
            enableTextSelection: true
        },
        forceFit: true
    });
    var store_RecrodDetail_cp = Ext.create('Ext.data.JsonStore', {
        model: 'RecrodDetail',
        pageSize: 20,
        proxy: {
            type: 'ajax',
            url: '/RecordInfor/loadRecordDetail',
            reader: {
                root: 'rows',
                type: 'json',
                totalProperty: 'total'
            }
        },
        autoLoad: true,
        listeners: {
            beforeload: function () {
                store_RecrodDetail_cp.getProxy().extraParams = {
                    ITEMNOATTRIBUTE: '成品',
                    RECORDINFORID: Ext.getCmp('s_combo_recordid').getValue(), ITEMNO: Ext.getCmp("field_ITEMNO").getValue(), HSCODE: Ext.getCmp('field_HSCODE').getValue(),
                    OPTIONS: Ext.getCmp('s_combo_optionstatus').getValue(), STATUS: Ext.getCmp("s_combo_status").getValue(),
                    ERROR: Ext.getCmp('chk_error').getValue() == true ? "1" : "0"
                }
            },
            load: function () {
                var total_cp = store_RecrodDetail_cp.getProxy().getReader().rawData.total;
                Ext.getCmp("tabpanel").items.items[1].setTitle("成品(" + total_cp + ")");
            }
        }
    });

    var pgbar_cp = Ext.create('Ext.toolbar.Paging', {
        id: 'pgbar_cp',
        displayMsg: '显示 {0} - {1} 条,共计 {2} 条',
        store: store_RecrodDetail_cp,
        displayInfo: true
    })
    gridpanel_cp = Ext.create('Ext.grid.Panel', {
        id: "gridpanel_cp",
        store: store_RecrodDetail_cp,
        height: 500,
        selModel: { selType: 'checkboxmodel' },
        bbar: pgbar_cp,
        enableColumnHide: false,
        columns: [
        { xtype: 'rownumberer', width: 35 },
        { header: 'ID', dataIndex: 'ID', hidden: true },
        //{ header: '变动状态', dataIndex: 'OPTIONS', width: 110 },
        //{ header: '申请状态', dataIndex: 'STATUS', width: 110 },
        { header: '账册号', dataIndex: 'CODE', width: 130 },
        { header: '项号', dataIndex: 'ITEMNO', width: 80 },
        { header: 'HS编码', dataIndex: 'HSCODE', width: 130 },
        { header: '附加码', dataIndex: 'ADDITIONALNO', width: 80 },
        { header: '项号属性', dataIndex: 'ITEMNOATTRIBUTE', width: 80 },
        { header: '商品名称', dataIndex: 'COMMODITYNAME', width: 200 },
        { header: '规格型号', dataIndex: 'SPECIFICATIONSMODEL', width: 200 },
        { header: '成交单位', dataIndex: 'UNIT', width: 80, renderer: renderOrder },
        //{ header: '报关行', dataIndex: 'CUSTOMERNAME', width: 150 },
        { header: '备注', dataIndex: 'REMARK', width: 150 }
        ],
        viewConfig: {
            enableTextSelection: true
        },
        forceFit: true
    });

    //====================================================================Go==================================================================
    var tbar_lj_Go = Ext.create('Ext.toolbar.Toolbar', {
        items: [
            {
                text: '<span class="icon iconfont" style="font-size:12px;color:#3071A9;">&#xe632;</span>&nbsp;<font color="#3071A9">修 改</font>',
                handler: function () { edit_task('gridpanel_lj_Go'); }
            },
               {
                   text: '<span class="icon iconfont" style="font-size:12px;color:#3071A9;">&#xe6d3;</span>&nbsp;<font color="#3071A9">删 除</font>',
                   handler: function () { delete_task('gridpanel_lj_Go', 'pgbar_lj_Go'); }
               },
               {
                   text: '<span class="icon iconfont" style="font-size:12px;color:#3071A9;">&#xe601;</span>&nbsp;<font color="#3071A9">申请表打印</font>',
                   handler: function () { print_task('gridpanel_lj_Go'); }
               }]

    });
    var store_RecrodDetail_lj_Go = Ext.create('Ext.data.JsonStore', {
        model: 'RecrodDetail',
        pageSize: 20,
        proxy: {
            type: 'ajax',
            url: '/RecordInfor/loadRecordDetail_Go',
            reader: {
                root: 'rows',
                type: 'json',
                totalProperty: 'total'
            }
        },
        autoLoad: true,
        listeners: {
            beforeload: function () {
                store_RecrodDetail_lj_Go.getProxy().extraParams = {
                    ITEMNOATTRIBUTE: '料件',
                    RECORDINFORID: Ext.getCmp('s_combo_recordid').getValue(), ITEMNO: Ext.getCmp("field_ITEMNO").getValue(), HSCODE: Ext.getCmp('field_HSCODE').getValue(),
                    OPTIONS: Ext.getCmp('s_combo_optionstatus').getValue(), STATUS: Ext.getCmp("s_combo_status").getValue(),
                    ERROR: Ext.getCmp('chk_error').getValue() == true ? "1" : "0"
                }
            },
            load: function () {
                var total_lj_Go = store_RecrodDetail_lj_Go.getProxy().getReader().rawData.total;
                Ext.getCmp("tabpanel").items.items[2].setTitle("料件_申请(" + total_lj_Go + ")");
            }
        }
    });

    var pgbar_lj_Go = Ext.create('Ext.toolbar.Paging', {
        id: 'pgbar_lj_Go',
        displayMsg: '显示 {0} - {1} 条,共计 {2} 条',
        store: store_RecrodDetail_lj_Go,
        displayInfo: true
    })
    gridpanel_lj_Go = Ext.create('Ext.grid.Panel', {
        id: 'gridpanel_lj_Go',
        store: store_RecrodDetail_lj_Go,
        height: 500,
        selModel: { selType: 'checkboxmodel' },
        tbar: tbar_lj_Go,
        bbar: pgbar_lj_Go,
        enableColumnHide: false,
        columns: [
        { xtype: 'rownumberer', width: 35 },
        { header: 'ID', dataIndex: 'ID', hidden: true },
        { header: '变动状态', dataIndex: 'OPTIONS', width: 110, renderer: renderOrder },
        { header: '申请状态', dataIndex: 'STATUS', width: 110, renderer: renderOrder },
        { header: '账册号', dataIndex: 'CODE', width: 130 },
        { header: '项号', dataIndex: 'ITEMNO', width: 80 },
        { header: 'HS编码', dataIndex: 'HSCODE', width: 130 },
        { header: '附加码', dataIndex: 'ADDITIONALNO', width: 80 },
        { header: '项号属性', dataIndex: 'ITEMNOATTRIBUTE', width: 80 },
        { header: '商品名称', dataIndex: 'COMMODITYNAME', width: 150 },
        { header: '规格型号', dataIndex: 'SPECIFICATIONSMODEL', width: 200 },
        { header: '成交单位', dataIndex: 'UNIT', width: 80, renderer: renderOrder },
        { header: '报关行', dataIndex: 'CUSTOMERNAME', width: 250 },
        { header: '备注', dataIndex: 'REMARK', width: 150 }
        ],
        listeners:
        {
            'itemdblclick': function (view, record, item, index, e) {
                edit_task('gridpanel_lj_Go');
            }
        },
        viewConfig: {
            enableTextSelection: true
        },
        forceFit: true
    });


    var tbar_cp_Go = Ext.create('Ext.toolbar.Toolbar', {
        items: [
            {
                text: '<span class="icon iconfont" style="font-size:12px;color:#3071A9;">&#xe632;</span>&nbsp;<font color="#3071A9">修 改</font>',
                handler: function () { edit_task('gridpanel_cp_Go'); }
            },
               {
                   text: '<span class="icon iconfont" style="font-size:12px;color:#3071A9;">&#xe6d3;</span>&nbsp;<font color="#3071A9">删 除</font>',
                   handler: function () { delete_task('gridpanel_cp_Go', 'pgbar_cp_Go'); }
               },
               {
                   text: '<span class="icon iconfont" style="font-size:12px;color:#3071A9;">&#xe601;</span>&nbsp;<font color="#3071A9">申请表打印</font>',
                   handler: function () { print_task('gridpanel_cp_Go'); }
               }]

    });

    var store_RecrodDetail_cp_Go = Ext.create('Ext.data.JsonStore', {
        model: 'RecrodDetail',
        pageSize: 20,
        proxy: {
            type: 'ajax',
            url: '/RecordInfor/loadRecordDetail_Go',
            reader: {
                root: 'rows',
                type: 'json',
                totalProperty: 'total'
            }
        },
        autoLoad: true,
        listeners: {
            beforeload: function () {
                store_RecrodDetail_cp_Go.getProxy().extraParams = {
                    ITEMNOATTRIBUTE: '成品',
                    RECORDINFORID: Ext.getCmp('s_combo_recordid').getValue(), ITEMNO: Ext.getCmp("field_ITEMNO").getValue(), HSCODE: Ext.getCmp('field_HSCODE').getValue(),
                    OPTIONS: Ext.getCmp('s_combo_optionstatus').getValue(), STATUS: Ext.getCmp("s_combo_status").getValue(),
                    ERROR: Ext.getCmp('chk_error').getValue() == true ? "1" : "0"
                }
            },
            load: function () {
                var total_cp_Go = store_RecrodDetail_cp_Go.getProxy().getReader().rawData.total;
                Ext.getCmp("tabpanel").items.items[3].setTitle("成品_申请(" + total_cp_Go + ")");
            }
        }
    });

    var pgbar_cp_Go = Ext.create('Ext.toolbar.Paging', {
        id: 'pgbar_cp_Go',
        displayMsg: '显示 {0} - {1} 条,共计 {2} 条',
        store: store_RecrodDetail_cp_Go,
        displayInfo: true
    })
    gridpanel_cp_Go = Ext.create('Ext.grid.Panel', {
        id: 'gridpanel_cp_Go',
        store: store_RecrodDetail_cp_Go,
        height: 500,
        selModel: { selType: 'checkboxmodel' },
        tbar: tbar_cp_Go,
        bbar: pgbar_cp_Go,
        enableColumnHide: false,
        columns: [
        { xtype: 'rownumberer', width: 35 },
        { header: 'ID', dataIndex: 'ID', hidden: true },
        { header: '变动状态', dataIndex: 'OPTIONS', width: 110, renderer: renderOrder },
        { header: '申请状态', dataIndex: 'STATUS', width: 110, renderer: renderOrder },
        { header: '账册号', dataIndex: 'CODE', width: 130 },
        { header: '项号', dataIndex: 'ITEMNO', width: 80 },
        { header: 'HS编码', dataIndex: 'HSCODE', width: 130 },
        { header: '附加码', dataIndex: 'ADDITIONALNO', width: 80 },
        { header: '项号属性', dataIndex: 'ITEMNOATTRIBUTE', width: 80 },
        { header: '商品名称', dataIndex: 'COMMODITYNAME', width: 150 },
        { header: '规格型号', dataIndex: 'SPECIFICATIONSMODEL', width: 200 },
        { header: '成交单位', dataIndex: 'UNIT', width: 80, renderer: renderOrder },
        { header: '报关行', dataIndex: 'CUSTOMERNAME', width: 250 },
        { header: '备注', dataIndex: 'REMARK', width: 150 }
        ],
        listeners:
        {
            'itemdblclick': function (view, record, item, index, e) {
                edit_task('gridpanel_cp_Go');
            }
        },
        viewConfig: {
            enableTextSelection: true
        },
        forceFit: true
    });

}

function Select() {
    Ext.getCmp('pgbar_lj').moveFirst(); Ext.getCmp('pgbar_cp').moveFirst();
    Ext.getCmp('pgbar_lj_Go').moveFirst(); Ext.getCmp('pgbar_cp_Go').moveFirst();
}

function Reset() {
    Ext.each(Ext.getCmp('formpanel').getForm().getFields().items, function (field) {
        field.reset();
    });
}

function Open(type) {
    if (type == 'A') {//新增申请
        opencenterwin("/RecordInfor/Create", 1600, 900);
    } else {
        var Compentid = "";
        if (Ext.getCmp("tabpanel").getActiveTab().id == "tab_0") { Compentid = "gridpanel_lj"; }
        if (Ext.getCmp("tabpanel").getActiveTab().id == "tab_1") { Compentid = "gridpanel_cp"; }

        var recs = Ext.getCmp(Compentid).getSelectionModel().getSelection();
        if (recs.length != 1) {
            Ext.Msg.alert("提示", "请选择一笔记录!");
            return;
        }

        Ext.Ajax.request({
            url: '/RecordInfor/Repeat_Task',
            params: { id: recs[0].get("ID") },
            success: function (response, success, option) {
                var res = Ext.decode(response.responseText);

                if (res.success) {//判断是否存在正在申请的记录
                    Ext.MessageBox.alert('提示', "此项号正在申请中！");
                }
                else {
                    if (type == 'U') {//变更申请                   
                        opencenterwin("/RecordInfor/Change?rid=" + recs[0].get("ID"), 1600, 900);
                    }
                    if (type == 'D') {//删除申请
                        opencenterwin("/RecordInfor/Delete?rid=" + recs[0].get("ID"), 1600, 900);
                    }
                }

            }
        });        
    }    
}

function edit_task(Compentid) {
    var recs = Ext.getCmp(Compentid).getSelectionModel().getSelection();
    if (recs.length != 1) {
        Ext.Msg.alert("提示", "请选择一笔修改记录!");
        return;
    }

    if (recs[0].get("OPTIONS") == "A") {
        opencenterwin("/RecordInfor/Create?id=" + recs[0].get("ID"), 1600, 900);
    }
    if (recs[0].get("OPTIONS") == "U") {
        opencenterwin("/RecordInfor/Change?id=" + recs[0].get("ID"), 1600, 900);
    }
    if (recs[0].get("OPTIONS") == "D") {
        opencenterwin("/RecordInfor/Delete?id=" + recs[0].get("ID"), 1600, 900);
    }
}

function delete_task(Compentid, Compentid_pgbar) {
    var recs = Ext.getCmp(Compentid).getSelectionModel().getSelection();
    if (recs.length == 0) {
        Ext.Msg.alert("提示", "请选择删除记录!");
        return;
    }
    var ids = ""; var bf = false;
    Ext.each(recs, function (rec) {
        if (rec.get("STATUS") != "0") { bf = true; }
        ids += rec.get("ID") + ",";
    });
    if (bf) {
        Ext.Msg.alert("提示", "只能删除状态为草稿的记录!");
        return;
    }
    ids = ids.substr(0, ids.length - 1);

    Ext.MessageBox.confirm("提示", "确定要删除所选择的记录吗？", function (btn) {
        if (btn == 'yes') {
            Ext.Ajax.request({
                url: '/RecordInfor/Delete_Task',
                params: { ids: ids },
                success: function (response, success, option) {
                    var res = Ext.decode(response.responseText);
                    var msgs = "";
                    if (res.success) { msgs = "删除成功！"; }
                    else { msgs = "删除失败！"; }

                    Ext.MessageBox.alert('提示', msgs, function (btn) {
                        Ext.getCmp(Compentid_pgbar).moveFirst();
                    });
                }
            });
        }
    });
}

function print_task(Compentid) {
    var recs = Ext.getCmp(Compentid).getSelectionModel().getSelection();
    if (recs.length == 0) {
        Ext.Msg.alert("提示", "请选择打印记录!");
        return;
    }
    var ids = ""; var bf = false;
    Ext.each(recs, function (rec) {
        if (rec.get("STATUS") == "0") { bf = true; }
        ids += rec.get("ID") + ",";
    });
    if (bf) {
        Ext.Msg.alert("提示", "状态为草稿 不可以打印，请重新选择需要打印的记录!");
        return;
    }
    ids = ids.substr(0, ids.length - 1);
    printitemno(ids);
    
}

function Export() {
    var recordid = Ext.getCmp('s_combo_recordid').getValue(); recordid = recordid == null ? "" : recordid;
    var itemno = Ext.getCmp('field_ITEMNO').getValue(); var hscode = Ext.getCmp('field_HSCODE').getValue();
    var options = Ext.getCmp('s_combo_optionstatus').getValue(); options = options == null ? "" : options;
    var status = Ext.getCmp('s_combo_status').getValue(); status = status == null ? "" : status;
    var error = Ext.getCmp('chk_error').getValue() == true ? "1" : "0";

    $('#e_options').val(JSON.stringify(optionstatus_js_data));
    $('#e_status').val(JSON.stringify(status_js_data));
    $('#e_unit').val(JSON.stringify(common_data_unit));

    var path = '/RecordInfor/Export?RECORDINFORID=' + recordid + '&ITEMNO=' + itemno + '&HSCODE=' + hscode + '&OPTIONS=' + options + '&STATUS=' + status + '&ERROR=' + error;
    $('#exportform').attr("action", path).submit();
}

