namespace Gu.Analyzers.Test.GU0032DisposeBeforeReassigningTests
{
    using System.Threading.Tasks;
    using NUnit.Framework;

    internal partial class HappyPath : HappyPathVerifier<GU0032DisposeBeforeReassigning>
    {
        [Test]
        public async Task CreateVariable()
        {
            var testCode = @"
    using System;
    using System.IO;

    public class Foo
    {
        public void Meh()
        {
            var stream = File.OpenRead(string.Empty);
        }
    }";

            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task AssigningPropertyInCtor()
        {
            var testCode = @"
using System;
using System.IO;

public class Foo
{
    public Foo()
    {
        this.Stream = File.OpenRead(string.Empty);
    }

    public Stream Stream { get; }
}";
            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task AssigningIndexerInCtor()
        {
            var testCode = @"
using System;
using System.Collections.Generic;

public class Foo
{
    private readonly List<int> ints = new List<int>();

    public Foo()
    {
        this[1] = 1;
    }

    public int this[int index]
    {
        get { return this.ints[index]; }
        set { this.ints[index] = value; }
    }
}";
            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task AssigningPropertyWithBackingFieldInCtor()
        {
            var testCode = @"
using System;
using System.IO;

public class Foo
{
    private Stream stream;

    public Foo()
    {
        this.Stream = File.OpenRead(string.Empty);
    }

    public Stream Stream
    {
        get { return this.stream; }
        set { this.stream = value; }
    }
}";
            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task AssigningFieldInCtor()
        {
            var testCode = @"
using System;
using System.IO;

public class Foo
{
    private readonly Stream stream;

    public Foo()
    {
        this.stream = File.OpenRead(string.Empty);
    }
}";
            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task FieldSwapCached()
        {
            var testCode = @"
using System.Collections.Generic;
using System.IO;

public class Foo
{
    private readonly Dictionary<int, Stream> Cache = new Dictionary<int, Stream>();

    private Stream current;

    public void SetCurrent(int number)
    {
        this.current = this.Cache[number];
    }
}";

            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task LocalSwapCached()
        {
            var testCode = @"
using System.Collections.Generic;
using System.IO;

public class Foo
{
    private readonly Dictionary<int, Stream> Cache = new Dictionary<int, Stream>();

    public void SetCurrent(int number)
    {
        var current = this.Cache[number];
        current = this.Cache[number + 1];
    }
}";

            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task DisposingVariable()
        {
            var testCode = @"
    using System;
    using System.IO;

    public class Foo
    {
        public void Meh()
        {
            var stream = File.OpenRead(string.Empty);
            stream.Dispose();
            stream = File.OpenRead(string.Empty);
        }
    }";

            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task AssigningInIfElse()
        {
            var testCode = @"
using System.IO;

public class Foo
{
    public void Meh()
    {
        Stream stream;
        if (true)
        {
            stream = File.OpenRead(string.Empty);
        }
        else
        {
            stream = File.OpenRead(string.Empty);
        }
    }
}";

            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [TestCase("stream.Dispose();")]
        [TestCase("stream?.Dispose();")]
        public async Task DisposeBeforeAssigningInIfElse(string dispose)
        {
            var testCode = @"
using System.IO;

public class Foo
{
    public void Meh()
    {
        Stream stream = File.OpenRead(string.Empty);
        if (true)
        {
            stream.Dispose();
            stream = File.OpenRead(string.Empty);
        }
        else
        {
            stream.Dispose();
            stream = File.OpenRead(string.Empty);
        }
    }
}";
            testCode = testCode.AssertReplace("stream.Dispose();", dispose);
            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task DisposingParameter()
        {
            var testCode = @"
using System;
using System.IO;

public class Foo
{
    public void Bar(Stream stream)
    {
        stream = File.OpenRead(string.Empty);
        stream?.Dispose();
        stream = File.OpenRead(string.Empty);
    }
}";
            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task OutParameterInCtor()
        {
            var testCode = @"
    using System;
    using System.IO;

    public class Foo
    {
        private Stream stream;

        public Foo()
        {
            TryGetStream(out stream);
        }

        public bool TryGetStream(out Stream stream)
        {
            stream = File.OpenRead(string.Empty);
            return true;
        }
    }";

            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task OutParameter()
        {
            var testCode = @"
    using System;
    using System.IO;

    public class Foo
    {
        public bool TryGetStream(out Stream stream)
        {
            stream = File.OpenRead(string.Empty);
            return true;
        }
    }";

            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [TestCase("Stream stream;")]
        [TestCase("Stream stream = null;")]
        [TestCase("var stream = (Stream)null;")]
        public async Task VariableSplitDeclarationAndAssignment(string declaration)
        {
            var testCode = @"
    using System;
    using System.IO;

    public class Foo
    {
        public void Meh()
        {
            Stream stream;
            stream = File.OpenRead(string.Empty);
        }
    }";

            testCode = testCode.AssertReplace("Stream stream;", declaration);
            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task DisposingFieldInCtor()
        {
            var testCode = @"
    using System;
    using System.IO;

    public class Foo
    {
        private readonly Stream stream = File.OpenRead(string.Empty);
        public Foo()
        {
            stream.Dispose();
            stream = File.OpenRead(string.Empty);
        }
    }";
            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task DisposingFieldInMethod()
        {
            var testCode = @"
    using System;
    using System.IO;

    public class Foo
    {
        private Stream stream;

        public void Meh()
        {
            this.stream.Dispose();
            this.stream = File.OpenRead(string.Empty);
        }
    }";
            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task ConditionallyDisposingFieldInMethod()
        {
            var testCode = @"
    using System;
    using System.IO;

    public class Foo
    {
        private Stream stream;

        public void Meh()
        {
            this.stream?.Dispose();
            this.stream = File.OpenRead(string.Empty);
        }
    }";
            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task ConditionallyDisposingUnderscoreFieldInMethod()
        {
            var testCode = @"
    using System;
    using System.IO;

    public class Foo
    {
        private Stream _stream;

        public void Meh()
        {
            _stream?.Dispose();
            _stream = File.OpenRead(string.Empty);
        }
    }";
            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task DisposingUnderscoreFieldInMethod()
        {
            var testCode = @"
    using System;
    using System.IO;

    public class Foo
    {
        private Stream _stream;

        public void Meh()
        {
            _stream.Dispose();
            _stream = File.OpenRead(string.Empty);
        }
    }";
            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task WithOptionalParameter()
        {
            var testCode = @"
namespace RoslynSandBox
{
    using System;
    using System.Collections.Generic;

    public class Foo
    {
        private IDisposable disposable;

        public Foo(IDisposable disposable)
        {
            this.disposable = Bar(disposable);
        }

        private static IDisposable Bar(IDisposable disposable, IEnumerable<IDisposable> disposables = null)
        {
            if (disposables == null)
            {
                return Bar(disposable, new[] { disposable });
            }

            return disposable;
        }
    }
}";
            await this.VerifyHappyPathAsync(testCode).ConfigureAwait(false);
        }

        [Test]
        public async Task ReproIssue71()
        {
            var code = @"
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxonomyWpf
{
	public class IndexedList<T> : IList<KeyValuePair<int, T>>
	{
		protected IList<T> decorated;

		public IndexedList(IList<T> decorated)
		{
			if(decorated == null)
				throw new ArgumentNullException(nameof(decorated));

			this.decorated = decorated;
		}

		public IEnumerator<KeyValuePair<int, T>> GetEnumerator()
		{
			return decorated.Select((element, index) => new KeyValuePair<int, T>(index, element)).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		void ICollection<KeyValuePair<int, T>>.Add(KeyValuePair<int, T> item)
		{
			Add(item.Value);
		}

		public void Add(T item)
		{
			decorated.Add(item);
		}

		public void Clear()
		{
			decorated.Clear();
		}

		bool ICollection<KeyValuePair<int, T>>.Contains(KeyValuePair<int, T> item)
		{
			return Contains(item.Value);
		}

		public bool Contains(T item)
		{
			return decorated.Contains(item);
		}

		public void CopyTo(KeyValuePair<int, T>[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		public bool Remove(KeyValuePair<int, T> item)
		{
			return decorated.Remove(item.Value);
		}

		public int Count => decorated.Count;
		public bool IsReadOnly => decorated.IsReadOnly;

		public int IndexOf(KeyValuePair<int, T> item)
		{
			return decorated.IndexOf(item.Value);
		}

		void IList<KeyValuePair<int, T>>.Insert(int index, KeyValuePair<int, T> item)
		{
			Insert(index, item.Value);
		}

		public void Insert(int index, T item)
		{
			decorated.Insert(index, item);
		}

		public void RemoveAt(int index)
		{
			decorated.RemoveAt(index);
		}
		public KeyValuePair<int, T> this[int index]
		{
			get { return new KeyValuePair<int, T>(index, decorated[index]); }
			set { decorated[index] = value.Value; }
		}
	}

	public class ObservableIndexedList<T> : IndexedList<T>, INotifyCollectionChanged
	{
		public ObservableIndexedList(ObservableCollection<T> decorated) : 
			base(decorated)
		{

		}

		public event NotifyCollectionChangedEventHandler CollectionChanged
		{
			add { ((ObservableCollection<T>)decorated).CollectionChanged += value; }
			remove { ((ObservableCollection<T>)decorated).CollectionChanged -= value; }
		}
	}
}";
            await this.VerifyHappyPathAsync(code).ConfigureAwait(false);
        }
    }
}