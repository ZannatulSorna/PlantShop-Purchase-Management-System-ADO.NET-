﻿using PlantShop.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PlantShop
{
    public partial class CustomerReport : Form
    {
        List<CustomerViewModel> _list;
        public CustomerReport(List<CustomerViewModel> list)
        {
            InitializeComponent();
            _list = list;
        }

        private void CustomerReport_Load(object sender, EventArgs e)
        {
            RtpCustomerInfo rpt = new RtpCustomerInfo();
            rpt.SetDataSource(_list);
            crystalReportViewer1.ReportSource = rpt;
            crystalReportViewer1.Refresh();
        }
    }
}
