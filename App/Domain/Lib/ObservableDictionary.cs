using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace App.Domain.Lib;

[Serializable]
public class ObservableKeyValuePair<TKey,TValue>:INotifyPropertyChanged
{
    #region properties
    private TKey key;
    private TValue value;

    public TKey Key
    {
        get => key;
        set
        {
            key = value;
            OnPropertyChanged("Key");
        }
    }

    public TValue Value
    {
        get => value;
        set
        {
            this.value = value;
            OnPropertyChanged("Value");
        }
    } 
    #endregion

    #region INotifyPropertyChanged Members

    [field:NonSerialized]
    public event PropertyChangedEventHandler? PropertyChanged;

    public void OnPropertyChanged(string name)
    {
        var handler = PropertyChanged;
        handler?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    #endregion
}

[Serializable]
public sealed class ObservableDictionary<TKey, TValue> : ObservableCollection<ObservableKeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>
{
    private readonly ObservableCollection<TKey> _keysCollection = new ObservableCollection<TKey>();

    public ObservableDictionary()
    {
        CollectionChanged += (sender, args) =>
        {
            if (args.NewItems != null)
            {
                foreach (ObservableKeyValuePair<TKey, TValue> item in args.NewItems)
                {
                    _keysCollection.Add(item.Key);
                    item.PropertyChanged += OnItemPropertyChanged!;
                }
            }
            if (args.OldItems != null)
            {
                foreach (ObservableKeyValuePair<TKey, TValue> item in args.OldItems)
                {
                    _keysCollection.Remove(item.Key);
                    item.PropertyChanged -= OnItemPropertyChanged!;
                }
            }
        };
    }

    private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "Key")
        {
            var item = (ObservableKeyValuePair<TKey, TValue>)sender;
            var index = _keysCollection.IndexOf(item.Key);
            if (index >= 0)
            {
                _keysCollection[index] = item.Key;
            }
        }
    }

    public ICollection<TKey> Keys => _keysCollection;

    #region IDictionary<TKey,TValue> Members

    public void Add(TKey key, TValue value)
    {
        if (ContainsKey(key))
        {
            throw new ArgumentException("The dictionary already contains the key");
        }
        base.Add(new ObservableKeyValuePair<TKey, TValue>() {Key = key, Value = value});
    }

    public bool ContainsKey(TKey key)
    {
        //var m=base.FirstOrDefault((i) => i.Key == key);
        var r = ThisAsCollection().FirstOrDefault((i) => Equals(key, i.Key));

        return !Equals(default(ObservableKeyValuePair<TKey, TValue>), r);
    }

    bool Equals<TKey>(TKey a, TKey b)
    {
        return EqualityComparer<TKey>.Default.Equals(a, b);
    }

    private ObservableCollection<ObservableKeyValuePair<TKey, TValue>> ThisAsCollection()
    {
        return this;
    }

    public bool Remove(TKey key)
    {
        var remove = ThisAsCollection().Where(pair => Equals(key, pair.Key)).ToList();
        foreach (var pair in remove)
        {
            ThisAsCollection().Remove(pair);
        }
        return remove.Count > 0;
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        value = default!;
        var pair = ThisAsCollection().FirstOrDefault(i => EqualityComparer<TKey>.Default.Equals(i.Key, key));
        
        if (pair == null) 
            return false;
        
        value = pair.Value;
        return true;
    }

    private ObservableKeyValuePair<TKey, TValue> GetKvpByTheKey(TKey key)
    {
        return ThisAsCollection().FirstOrDefault(i => i.Key!.Equals(key))!;
    }

    public ICollection<TValue> Values => (from i in ThisAsCollection() select i.Value).ToList();

    public TValue this[TKey key]
    {
        get
        {
            if (!TryGetValue(key,out var result))
            {
                throw new ArgumentException("Key not found");
            }
            return result;
        }
        set
        {
            if (ContainsKey(key))
            {
                GetKvpByTheKey(key).Value = value;
            }
            else
            {
                Add(key, value);
            }
        }
    }

    #endregion

    #region ICollection<KeyValuePair<TKey,TValue>> Members

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        Add(item.Key, item.Value);
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        var r = GetKvpByTheKey(item.Key);
        if (Equals(r, default(ObservableKeyValuePair<TKey, TValue>)))
        {
            return false;
        }
        return Equals(r.Value, item.Value);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    public bool IsReadOnly => false;

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        var r = GetKvpByTheKey(item.Key);
        if (Equals(r, default(ObservableKeyValuePair<TKey, TValue>)))
        {
            return false;
        }
        if (!Equals(r.Value,item.Value))
        {
            return false ;
        }
        return ThisAsCollection().Remove(r);
    }

    #endregion

    #region IEnumerable<KeyValuePair<TKey,TValue>> Members

    public new IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return (from i in ThisAsCollection() select new KeyValuePair<TKey, TValue>(i.Key, i.Value)).ToList().GetEnumerator();
    }

    #endregion
}