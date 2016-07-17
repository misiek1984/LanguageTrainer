using System;
using System.Collections.Generic;
using MK.Utilities;

namespace MK.UI.WPF
{
    public class TypedVM<T> : ViewModelBase
    {
        private T _entity;
        public T Entity
        {
            get { return _entity; }
            set 
            {
                _entity = value;
                Notify(() => Entity);
            }
        }

        public TypedVM()
        {
        }

        public TypedVM(T entity)
        {
            entity.NotNull("entity");
            Entity = entity;
        }

        public TypedVM(ViewModelBase parent)
            : base(parent)
        {
        }

        public TypedVM(ViewModelBase parent, T entity)
            : base(parent)
        {
            entity.NotNull("entity");
            Entity = entity;
        }
    }

    public class TypedVM<T, K> : TypedVM<T>
    {
        private K _entity2;
        public K Entity2
        {
            get { return _entity2; }
            set
            {
                _entity2 = value;
                Notify(() => Entity2);
            }
        }

        public TypedVM()
        {
        }

        public TypedVM(T entity, K entity2)
            :base(entity)
        {
            entity2.NotNull("entity2");
            Entity2 = entity2;
        }

        public TypedVM(ViewModelBase parent)
            : base(parent)
        {
        }

        public TypedVM(ViewModelBase parent, T entity, K entity2)
            : base(parent, entity)
        {
            entity2.NotNull("entity2");
            Entity2 = entity2;
        }
    }
}
