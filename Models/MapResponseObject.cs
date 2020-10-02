using AutoWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LudocusApi.Models
{
    public class MapResponseObject
    {
        [AutoWrapperPropertyMap(Prop.Result)]
        public object Data { get; set; }

        [AutoWrapperPropertyMap(Prop.ResponseException)]
        public object Error { get; set; }

        [AutoWrapperPropertyMap(Prop.ResponseException_ExceptionMessage)]
        public string Message { get; set; }
    }
}
