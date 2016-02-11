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
        private Level[] _levels;
        private string _source;
        private Regex _sourceRegex;
        public bool Final { get; set; }

        public string Source
        {
            get { return _source; }
            set
            {
                _source = value;
                _sourceRegex = null;
            }
        }

        public Level? MinLevel { get; set; }
        public Level? MaxLevel { get; set; }

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

        public virtual Level[] Levels
        {
            get { return _levels ?? (_levels = GetLevelsInRange()); }
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
            Final = Extensions.ParseBooleanSetting(xml.GetAttributeValue("final", "isfinal"));

            if (!levels.IsNullOrWhitespace())
                Levels = levels.Split(new[] {','}).Select(Enums.Parse<Level>).ToArray();

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
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            var hasLevel = Levels.Contains(entry.Level);
            var matchesSource = SourceRegex == null || SourceRegex.IsMatch(entry.Source);
            var shouldWrite = hasLevel && matchesSource;
            return shouldWrite;
        }

        protected T ConstructOrDefault<T>([NotNull] string name, IEnumerable<Assembly> assemblies = null)
        {
            return Attempt.Get(() =>
                Types.Implementing<T>(assemblies)
                    .Search(name)
                    .FirstOrDefault()
                    .ConstructOrDefault<T>()).Value;
        }
    }
}