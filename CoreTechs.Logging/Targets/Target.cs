using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace CoreTechs.Logging.Targets
{
    public abstract class Target
    {
        public string Source { get; set; }
        public Level? MinLevel { get; set; }
        public Level? MaxLevel { get; set; }

        private Regex _sourceRegex;
        private Regex SourceRegex
        {
            get
            {
                // code for wildcard > regex @ http://stackoverflow.com/a/6907849/64334

                return Source.IsNullOrWhitespace()
                    ? null
                    : _sourceRegex
                      ?? (_sourceRegex = new Regex("^" + Regex.Escape(Source)
                          .Replace(@"\*", ".*")
                          .Replace(@"\?", ".") + "$", RegexOptions.Compiled));
            }
        }

        private Level[] _levels;
        public virtual Level[] Levels
        {
            get
            {
                return _levels ?? (_levels = GetLevelsInRange());
            }
            set
            {
                MinLevel = null;
                MaxLevel = null;
                _levels = value ?? Enums.GetAll<Level>().ToArray();
            }
        }

        private Level[] GetLevelsInRange()
        {
            return Enums.GetAll<Level>()
                .Where(l => (MinLevel == null || l >= MinLevel) && (MaxLevel == null || l <= MaxLevel))
                .ToArray();
        }

        public abstract void Write(LogEntry entry);

        internal void ConfigureInternal(XElement xml)
        {
            var levels = xml.GetAttributeValue("level", "levels");
            var minlevel = xml.GetAttributeValue("minlevel");
            var maxlevel = xml.GetAttributeValue("maxlevel");
            Source = xml.GetAttributeValue("source");

            if (!levels.IsNullOrWhitespace())
                Levels = levels.Split(new[] { ',' }).Select(Enums.Parse<Level>).ToArray();

            if (!minlevel.IsNullOrWhitespace())
                MinLevel = Enums.Parse<Level>(minlevel);

            if (!maxlevel.IsNullOrWhitespace())
                MaxLevel = Enums.Parse<Level>(maxlevel);

            var configurable = this as IConfigurable;
            if (configurable != null)
                configurable.Configure(xml);
        }


        internal bool ShouldWriteInternal([NotNull] LogEntry entry)
        {
            if (entry == null) throw new ArgumentNullException("entry");
            return Levels.Contains(entry.Level)
                   && (SourceRegex == null || SourceRegex.IsMatch(entry.Source));
        }

        protected T ConstructOrDefault<T>([NotNull] string name, IEnumerable<Assembly> assemblies = null)
        {
            return TryTo.Get(() =>
                Types.Implementing<T>(assemblies)
                    .Search(name)
                    .FirstOrDefault()
                    .ConstructOrDefault<T>()).Value;
        }
    }

}