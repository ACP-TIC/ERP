﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace entity.Controller
{
   public  class DataFilter
    {
        public  DateTime Start_Range
        {
            get { return _start_Range; }
            set
            {
                if (_start_Range != value)
                {
                    _start_Range = value;
                }
            }
        }
        private  DateTime _start_Range = DateTime.Now.AddDays(-7);

        public  DateTime End_Range
        {
            get { return _end_Range; }
            set
            {
                if (_end_Range != value)
                {
                    _end_Range = value;
                }
            }
        }
        private  DateTime _end_Range = DateTime.Now.AddDays(+1);
    }
}
