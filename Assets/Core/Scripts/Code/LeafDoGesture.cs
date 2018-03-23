using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections;
using UnityEngine;

namespace TreeSharpPlus
{
    public class LeafDoGesture : Node
    {
        protected Action<float> func_noReturn = null;
        protected float param = 0;
        
        public LeafDoGesture(
            Action<float> function)
        {
            this.func_noReturn = function;
        }
        
        public void SetParam(float param)
        {
            this.param = param;
        }
        
        public override RunStatus Terminate()
        {
            RunStatus curStatus = this.StartTermination();
            if (curStatus != RunStatus.Running)
                return curStatus;
            
            return this.ReturnTermination(RunStatus.Success);
        }

        public override IEnumerable<RunStatus> Execute()
        {
            this.func_noReturn.Invoke( param);
            yield return RunStatus.Success;
            yield break;
        }
    }
}