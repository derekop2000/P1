﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Job
{
    public interface IJob
    {
        void Execute();
    }

    public class Job : IJob
    {
        Action _action;
        public Job(Action action)
        {
            _action = action;
        }
        public void Execute()
        {
            _action();
        }
    }

    public class Job<T1> : IJob
    {
        Action<T1> _action;
        T1 _t1;
        public Job(Action<T1> action,T1 t1)
        {
            _action = action;
            _t1 = t1;
        }
        public void Execute()
        {
            _action(_t1);
        }
    }

    public class Job<T1,T2> : IJob
    {
        Action<T1,T2> _action;
        T1 _t1;
        T2 _t2;
        public Job(Action<T1,T2> action, T1 t1,T2 t2)
        {
            _action = action;
            _t1 = t1;
            _t2 = t2;
        }
        public void Execute()
        {
            _action(_t1,_t2);
        }
    }

    public class Job<T1, T2,T3> : IJob
    {
        Action<T1, T2,T3> _action;
        T1 _t1;
        T2 _t2;
        T3 _t3;
        public Job(Action<T1, T2,T3> action, T1 t1, T2 t2,T3 t3)
        {
            _action = action;
            _t1 = t1;
            _t2 = t2;
            _t3 = t3;
        }
        public void Execute()
        {
            _action(_t1, _t2,_t3);
        }
    }

    public class Job<T1, T2, T3,T4> : IJob
    {
        Action<T1, T2, T3,T4> _action;
        T1 _t1;
        T2 _t2;
        T3 _t3;
        T4 _t4;
        public Job(Action<T1, T2, T3,T4> action, T1 t1, T2 t2, T3 t3,T4 t4)
        {
            _action = action;
            _t1 = t1;
            _t2 = t2;
            _t3 = t3;
            _t4 = t4;
        }
        public void Execute()
        {
            _action(_t1, _t2, _t3,_t4);
        }
    }
}
