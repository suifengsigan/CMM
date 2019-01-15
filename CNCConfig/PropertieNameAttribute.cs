using System;

namespace CNCConfig
{
    public class PropertieNameAttribute : Attribute
    {
        public String Name { get; set; }

        public PropertieNameAttribute()
        {

        }

        public PropertieNameAttribute(String name)
        {
            Name = name;
        }
    }
}