using System.Collections.Generic;
using Localizer.DataModel;
using Localizer.DataModel.Default;
using Localizer.ServiceInterfaces;

namespace Localizer.Services.File
{
    public sealed class BasicFileUpdate<T> : IFileUpdateService<T> where T : IFile
    {
        public void Update(T oldFile, T newFile, IUpdateLogService logger)
        {
            if (oldFile.GetType() != typeof(T) || newFile.GetType() != typeof(T))
            {
                return;
            }

            foreach (var prop in typeof(T).ModTranslationOwnerField())
            {
                dynamic oldEntries = prop.GetValue(oldFile);
                dynamic newEntries = prop.GetValue(newFile);

                foreach (var newEntryKey in newEntries.Keys)
                {
                    if (oldEntries.ContainsKey(newEntryKey))
                    {
                        UpdateEntry(newEntryKey, oldFile.GetValue(newEntryKey), newFile.GetValue(newEntryKey), logger);
                    }
                    else
                    {
                        logger.Add($"[{newEntryKey}]");
                        dynamic entry = newEntries[newEntryKey].Clone();
                        oldEntries.Add(newEntryKey, entry);
                    }
                }

                var removed = new List<string>();
                foreach (var k in oldEntries.Keys)
                {
                    if (!newEntries.ContainsKey(k))
                    {
                        removed.Add(k);
                    }
                }

                foreach (var r in removed)
                {
                    logger.Remove($"[{r}]");
                }
                
                prop.SetValue(oldFile, oldEntries);
            }
        }

        internal void UpdateEntry(string key, IEntry oldEntry, IEntry newEntry, IUpdateLogService logger)
        {
            foreach (var prop in oldEntry.GetType().ModTranslationProp())
            {
                var o = prop.GetValue(oldEntry) as BaseEntry;
                var n = prop.GetValue(newEntry) as BaseEntry;

                if (o.Origin != n.Origin)
                {
                    logger.Change($"{key}'s {prop.Name}\r\n[Old: \"{o.Origin}\"]\r\n => \r\n[New: \"{n.Origin}\"]\r\n");

                    o.Origin = n.Origin;
                }
            }
        }

        public void Dispose()
        {
        }
    }
}
