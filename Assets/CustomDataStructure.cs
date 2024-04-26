using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Custom
{
    
    public class CustomDataStructure<T>
    {
        private Dictionary<object, Guid> elementToIdMap;    // here 'object' is one element from 'Data' ('Data' of <T> type)
        private Dictionary<Guid, object> idToDataMap;       // here 'object' is the 'Data' itself

        public CustomDataStructure(params T[] elements)
        {
            elementToIdMap = new();
            idToDataMap = new();

            foreach (T element in elements)
            {
                if (!ElementIsValid(element))
                {
                    throw new ArgumentException("'CustomDataStructure' got invalid input");
                }

                PropertyInfo[] properties = typeof(T).GetProperties();
                // add to 'elementToIdMap' proper element
            }
        }



        private bool ElementIsValid(T element)
        {
            PropertyInfo[] properties = typeof(T).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                Type propertyType = property.PropertyType;
                if (!propertyType.IsGenericType)                                    return false;
                if (propertyType.GetGenericTypeDefinition() != typeof(ValueTuple))  return false;

                Type[] argumentTypes = propertyType.GetGenericArguments();
                if (propertyType.GetGenericArguments().Length != 2)                 return false;
                if (argumentTypes[1] != typeof(bool))                               return false;

            }

            return true;
        }

        private bool CanBeKey((string value, bool canBeKey) elementProperty)
        {
            // todo

            return false;
        }
    }


    public class CustomTuple
    {
        public (string value, bool canBeKey) name { get; set; }
        public (int value, bool canBeKey) age { get; set; }
        public (bool value, bool canBeKey) isNormal { get; set; }


        public CustomTuple(string name, int age, bool isNormal)
        {
            this.name       = (name,        canBeKey: true);
            this.age        = (age,         canBeKey: true);
            this.isNormal   = (isNormal,    canBeKey: false);
        }
    }



    public class Main
    {
        CustomDataStructure<CustomTuple> customDataStructure;

        public Main()
        {
            customDataStructure = new(
                new CustomTuple(name: "Hanna", age: 21, isNormal: false),
                new CustomTuple(name: "Anna", age: 22, isNormal: false)
            );

            // here print to console something
        }
    }
}
