using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadImpact
{
    public class Arguments
    {

        private readonly IList<string> _args;

        public Arguments(IEnumerable<string> arguments)
        {
            _args = arguments.ToList();
        }


        public bool IsSwitchPresent(string argSwitch)
        {
            return _args.IndexOf(argSwitch) >= 0;
        }


        public string GetSwitchValue(string argSwitch)
        {
            return GetSwitchValue<string>(argSwitch);
        }

        public string TryGetSwitchValue(string argSwitch, out bool sucess)
        {
            return TryGetSwitchValue<string>(argSwitch, out sucess);
        }

        public string TryGetSwitchValue(string argSwitch)
        {
            bool sucess;
            return TryGetSwitchValue<string>(argSwitch, out sucess);
        }

        public T GetSwitchValue<T>(string argSwitch)
        {
            var index = _args.IndexOf(argSwitch);
            if (index < 0 || (_args.Count - 1) <= index) throw new ArgumentException(string.Format("Invalid switch provided {0}", argSwitch));
            return (T)Convert.ChangeType(_args[index + 1], typeof(T));
        }

        public T TryGetSwitchValue<T>(string argSwitch)
        {
            var result = false;
            return TryGetSwitchValue<T>(argSwitch, out result);
        }

        public T TryGetSwitchValue<T>(string argSwitch, out bool sucess)
        {
            sucess = true;
            T result;
            try
            {
                result = GetSwitchValue<T>(argSwitch);
            }
            catch (Exception)
            {
                result = default(T);
                sucess = false;
            }
            return result;
        }

        public IList<string> Items
        {
            get { return _args; }
        }
    }
}
