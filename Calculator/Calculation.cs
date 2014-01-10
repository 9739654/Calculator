//#define DERIVATIVES
#define FUNCTIONS
#define FACTORIALS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculator
{
	public class InvalidTokenException : Exception
	{
		public InvalidTokenException(string message) : base(message) { }
	}

	public static class ExtensionMethods
	{
		public static char Peek(this Queue<char> q, char c)
		{
			return q.Count() > 0 ? q.Peek() : c;
		}
		public static bool AddOnce<T>(this List<T> l, T item)
		{
			if (l.Contains(item))
				return false;
			else
				l.Add(item);
			return true;
		}
		public static List<Number> getNumbers(this List<Operation> l)
		{
			List<Number> numbers = new List<Number>();
			foreach(Operation o in l)
				if (o.GetType() == typeof(Number))
					numbers.Add((Number)o);
			return numbers;
		}
		public static List<Operation> getNaN(this List<Operation> l)
		{
			List<Operation> nans = new List<Operation>();
			foreach (Operation o in l)
				if (o.GetType() != typeof(Number))
					nans.Add(o);
			return nans;
		}
	}

	class KV<Type, Object>
	{
		public Type key;
		public Object val;

		KV(Type key, Object val)
		{
			this.key = key;
			this.val = val;
		}
	}

	class  Token
	{
		// kind + value:
		public const char NUMBER		= 'd';
		public const char NAME			= 's';
		public const char ERROR			= 'e';
		// ony kind, without value:
		public const char UNKNOWN		= 'u';
		public const char END			= '\n';
		// operators:
		public const char BRACKETL		= '(';
		public const char BRACKETR		= ')';
		public const char ADDITION		= '+';
		public const char DIVISION		= '/';
		public const char EXPONENTIATION	= '^';
		public const char MULTIPLICATION	= '*';
		public const char SUBSTRACTION	= '-';
		public const char DERIVATIVE	= '\'';
		public const char FACTORIAL		= '!';

		private Queue<char> q;
		private int initQueueSize;
		public char	kind { get { return _kind; } set { _kind = value; } }
		private char _kind;
		public double	dval { get; set; }
		public string	sval {
			get { return _sval; }
			set {
				_sval = value;
				if (kind!=ERROR || kind!=NAME) kind = UNKNOWN;
			}
		}
		private string _sval;
		public int	pos { get; private set;}

		public Token()
		{
			kind = UNKNOWN;
		}
		public Token(double d)
		{
			kind = NUMBER;
			dval = d;
		}
		public Token(char k)
		{
			kind = k;
		}
		public Token(char kind, Object value)
		{
			this.kind = kind;
			if (kind == NUMBER) dval = (double) value;
			else sval = (string) value;

		}
		public Token(Queue<char> queue)
		{
			q = queue;
			initQueueSize = queue.Count;
			setPos();
			readToken(queue);
		}
		public	bool	isPlus()
		{
			return kind==ADDITION ? true : false;
		}
		public	bool	isMinus()
		{
			return kind==SUBSTRACTION ? true : false;
		}
		public	bool	isMultiply()
		{
			return kind==MULTIPLICATION ? true : false;
		}
		public	bool	isDivide()
		{
			return kind == DIVISION ? true : false;
		}
		public	bool	isNumber()
		{
			return kind==NUMBER ? true : false;
		}
		public	bool	isName()
		{
			return kind==NAME && sval.Length>0 ? true : false;
		}
		public	bool	isBracket()
		{
			return kind==BRACKETR || kind==BRACKETL ? true : false;
		}
		public	bool	isBracketL()
		{
			return kind==BRACKETL ? true : false;
		}
		public	bool	isBracketR()
		{
			return kind == BRACKETR ? true : false;
		}
		public	bool	isError()
		{
			return kind==ERROR ? true : false;
		}
		private void	setPos()
		{
			pos = initQueueSize - q.Count();
		}
		private void	readToken(Queue<char> q)
		{
			char c = q.Peek(END);
			while (isWhitespace(c) && c!='\n')
			{
				q.Dequeue();
				c = q.Peek(END);
			}
			if (isDigit(c))
			{
				double doubleval;
				bool b = readNumber(q, out doubleval);
				if (b)
				{
					dval = doubleval;
					kind = NUMBER;
				}
				else
				{
					throw new InvalidTokenException("char: "+c+" token: "+doubleval);
					dval = 0;
					kind = ERROR;
					sval = "ERROR: Token.readNumber";
					setPos();
					return;
				}
			}
			else if (isLetter(c))
			{
				string stringval;
				bool b = readName(q, out stringval);
				if (b)
				{
					sval = stringval;
					kind = NAME; 
				}
				else
				{
					throw new InvalidTokenException("token: "+stringval);
					kind = ERROR;
					sval = "nierozpoznany string";
					setPos();
					return;
				}
			}
			else if (c == END)
				kind = END;
			/*else if (c == DERIVATIVE)
				kind = DERIVATIVE;*/
			else
			{
				q.Dequeue();
				switch (c)
				{
					case '\n':	kind = END; break;
					case '(':	kind = BRACKETL; break;
					case ')':	kind = BRACKETR; break;
					case '+':	kind = ADDITION; break;
					case '-':	kind = SUBSTRACTION; break;
					case '*':	kind = MULTIPLICATION; break;
					case '/':	kind = DIVISION; break;
					case '^':	kind = EXPONENTIATION; break;
					case '!':	kind = FACTORIAL; break;
					case '\'':	kind = DERIVATIVE; break; // DERIVATIVE
					
					default:
						throw new InvalidTokenException("char: "+c);
				}
			}

		}
		private bool	readNumber(Queue<char> q, out double d)
		{
			int i;
			bool n = readInt(q, out i);
			if (!n)
			{
				d = 0;
				return false;
			}
			char c = q.Peek(END);
			if (c.Equals('.'))
			{
				c = q.Dequeue();	// kropka, która była tylko .Peek()
				c = q.Peek(END);
				if (!isDigit(c))
				{
					d = 0.0;
					throw new Exception("digit after '.' expected");
				}
				else
				{
					d = i + readDec(q);
					return true;
				}
			}
			else
			{
				d = i;
				return true;
			}
		}
		private double	readDec(Queue<char> q)
		{
			int n = 10;
			double d = 0;
			char c = q.Peek(END);
			while (isDigit(c))
			{
				q.Dequeue();
				d += (c - 48.0) / n;
				n *= 10;
				c = q.Peek(END);
			}
			return d;
		}
		private bool	readInt(Queue<char> q, out int i)
		{
			char c = q.Peek(END);
			string s = "";
			while (isDigit(c))
			{
				q.Dequeue();
				s += c.ToString();
				c = q.Peek(END);
			}
			if (s.Length == 0)
			{
				i = 0;
				return false;
			}
			else
			{
				return int.TryParse(s, out i);
			}
		}
		private bool	readName(Queue<char> q, out string s)
		{
			char c = q.Peek(END);
			string a = "";
			while (isLetter(c)/* || isDigit(c)*/)
			{
				q.Dequeue();
				a += c.ToString();
				c = q.Peek(END);
			}
			s = a;
			if (s.Length > 0)
			{
				return true;
			}
			else
			{
				throw new Exception("internal error: not a name");
			}
		}
		private bool	isDigit(char c)
		{
			return Char.IsDigit(c);
		}
		private bool	isLetter(char c)
		{
			return Char.IsLetter(c);
		}
		private bool	isWhitespace(char c)
		{
			return ((int)c<=32)?true:false;
		}
		public	override string ToString()
		{
			if (kind == NUMBER) return dval.ToString();

			if (kind == NAME || kind == ERROR) return sval;

			return kind.ToString();
		}
		public	static	Token operator +(Token t1, Token t2)
		{
			if (t1.kind == NUMBER && t2.kind == NUMBER)
			{
				return new Token(t1.dval + t2.dval);
			}
			return new Token();
		}
		public	static	Token operator -(Token t1, Token t2)
		{
			if (t1.kind == NUMBER && t2.kind == NUMBER)
			{
				return new Token(t1.dval - t2.dval);
			}
			return new Token();
		}
		public	static	Token operator *(Token t1, Token t2)
		{
			if (t1.kind == NUMBER && t2.kind == NUMBER)
			{
				return new Token(t1.dval * t2.dval);
			}
			return new Token();
		}
		public	static	Token operator /(Token t1, Token t2)
		{
			if (t1.kind == NUMBER && t2.kind == NUMBER)
			{
				if (t2.dval == 0.0)
				{
					return new Token(ERROR, "Dzielenie przez zero!");
				}
				return new Token(t1.dval / t2.dval);
			}
			return new Token();
		}
		public Value toValue()
		{
			if (kind == NUMBER)
				return new Number(dval);
			else
				return new Variable(sval);
		}
	}

	class TokenStream
	{
		Queue<Token>	buff;
		Queue<char>		input;
		bool			finished;

		public	TokenStream(string input)
		{
			this.input = new Queue<char>();
			foreach (char c in input)
				this.input.Enqueue(c);
			buff = new Queue<Token>();
		}
		private	Token	readNext()
		{
			if (!finished)
			{
				Token t = new Token(this.input);
				if (t.kind == Token.END)
					finished = true;
				/*if (t.kind == Token.NAME)
				{
					t.kind = Token.ERROR;
					t.sval = "string not allowed";
				}*/
				return t;
			}
			else
				return new Token(Token.END, '\n');
		}
		public	Token	get()
		{
			if (buff.Count > 0)
				return buff.Dequeue();
			else
				return readNext();
		}
		public	void	putback(Token t)
		{
			buff.Enqueue(t);
		}
	}

	public abstract class Operation
	{
		//public virtual List<Equation> elements { get; set; }
		public Operation left { get; protected set; }
		public Operation right { get; protected set; }
		public string op = "";
		public bool rec = false;

		public virtual Operation reciprocal()
		{
			rec = !rec;
			return this;
		}

		public abstract bool isZero();				// true if zero otherwise false
		public abstract bool isOne();					// true if one otherwise false
		public virtual List<Number> getNumbers()
		{
			var r = new List<Number>();
			if (left.isNumber())
				r.Add((Number)left);
			if (right.isNumber())
				r.Add((Number)right);
			return r;
		}
		public virtual List<Operation> getNaN()
		{
			var r = new List<Operation>();
			if (!left.isNumber())
				r.Add(left);
			if (!right.isNumber())
				r.Add(right);
			return r;
		}
		public virtual bool isNumber() { return false; }
		public virtual bool isVaribale() { return false; }
		public virtual string toTree(string tabs="") {
			string r = tabs + op + '\n';
			r += tabs + '\t' +	left.toTree(tabs+'\t') + '\n';
			r += tabs + '\t' + right.toTree(tabs + '\t') + '\n';
			return r+'\n';
		}
		public abstract bool isPositive();
		public abstract bool isNegative();
		public virtual bool isReciprocal() { return rec; }
		public static Operation operator +(Operation e1, Operation e2)
		{
			return new Sum().sum(e1,e2);
		}
		public static Operation operator -(Operation e)
		{
			return new Minus().minus(e);
		}
		public static Operation operator -(Operation e1, Operation e2)
		{
			return new Sum().sum(e1,new Minus().minus(e2));
		}
		public static Operation operator *(Operation e1, Operation e2)
		{
			return new Multip().multiply(e1, e2);
		}
		public static Operation operator /(Operation e1, Operation e2)
		{
			return new Multip().multiply(e1,e2.reciprocal());
		}
		public static Operation operator ^(Operation e1, Operation e2)
		{
			return new Power().power(e1, e2);
		}
		public static Operation operator !(Operation e)
		{
			return e.reciprocal();
		}
		public static bool operator ==(Operation o1, Operation o2)
		{
			if (o1.GetType() == o2.GetType() && o1.left == o2.left && o1.right == o2.right)
				return true;
			else
				return false;
		}
		public static bool operator !=(Operation o1, Operation o2)
		{
			if (o1.GetType() == o2.GetType() && o1.left == o2.left && o1.right == o2.right)
				return false;
			else
				return true;
		}

		public abstract Operation derivative(string x);	// calculates the derivative
		public override bool Equals(object obj)
		{
			Operation ope;
			if (obj.GetType() == typeof(Operation))
				ope = (Operation)obj;
			else
				return false;

			if (ope.GetType() == GetType() && ope.left.Equals(left) && ope.right.Equals(right))
				return true;
			else
				return false;
		}
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
		public override string ToString()
		{
			return left.ToString() + op + right.ToString();
		}
	}

	class Minus : Operation
	{
		internal Operation minus(Operation e)
		{
			if (e.GetType() == typeof(Minus))
				return e;
			else if (e.GetType() == typeof(Number))
				if (((Number)e).val < 0)
					return e;
				else
				{
					((Number)e).val = -((Number)e).val;
					return e;
				}
			else if (e.GetType() == typeof(Multip))
			{
				List <Number> numbers = e.getNumbers();
				if (numbers.Count>0)
				{
					Number n = numbers.First();
					n.val = -n.val;
					return e;
				}
			}
			
			left = e;
			return this;
		}
		public override Operation derivative(string x)
		{
			left = left.derivative(x);
			return this;
		}
		public override bool isNegative()
		{
			return true;
		}
		public override bool isPositive()
		{
			return false;
		}
		public override bool isOne()
		{
			return left.isOne();
		}
		public override bool isZero()
		{
			return left.isZero();
		}
		public override Operation reciprocal()
		{
			rec = !rec;
			return this;
		}
		
		public override string ToString()
		{
			return '-'+left.ToString();
		}
	}

	class Sum : Operation
	{
		public Sum()
		{
			op = "+";
			//elements = new List<Equation>();
		}

		public override Operation reciprocal()
		{
            rec = !rec;
            return this;
		}
		public override bool isOne()
		{
            return false;
		}
		public override bool isZero()
		{
            return left.isZero() && right.isZero();
		}
		public override bool isNegative()
		{
			return !isPositive();
		}
		public override bool isPositive()
		{
			if (left.isNegative() && right.isNegative()
				|| left.isPositive() && right.isPositive()
				|| left.isVaribale() && right.isVaribale()
				/*|| left.isVaribale() && right.isPositive()
				 *|| left.isPositive() && right.isVaribale()*/)
				return true;
			else
				return false;
		}
		public override Operation derivative(string x)
		{
			//TODO sum derivative
			throw new NotImplementedException();
		}
        private Operation sum(Number n, Sum s)
        {
            if (s.getNumbers().Count == 1)
            {
                s.getNumbers().First().val += n.val;
                return s;
            }
            else
                return sumThis(s, n);
        }
        private Operation sum(Number n1, Number n2)
        {
            return new Number(((Number)n1).val + ((Number)n2).val);
        }
        private Operation sum(Sum s1, Sum s2)
        {
            var lnum = s1.getNumbers().First();
            var rnum = s2.getNumbers().First();
            var nan = s1.getNaN();
            nan.AddRange(s2.getNaN());
            
            switch (nan.Count)
            {
                case 2:
                    {
                        left = new Number(lnum.val + rnum.val);
                        right = new Sum().sum(nan[0], nan[1]);
                        return this;
                    }
                case 3:
                    {
                        left = object.ReferenceEquals(lnum, null) ? rnum : lnum;
                        right = (object.ReferenceEquals(lnum, left)) ? (new Sum().sum(nan[0], s2)) : (new Sum().sum(s1, nan[0]));
                        return this;
                    }
                case 4:
                    {
                        left = s1;
                        right = s2;
                        return this;
                    }
                default:
                    return new Variable("not a number?");
            }
        }
        private Operation sumThis(Operation left, Operation right)
        {
			// suma dwóch identycznych zmiennych => iloczyn zmiennej przez 2
			if (left.GetType() == typeof(Variable) && right.GetType() == typeof(Variable))
			{
				Variable v1 = (Variable)left;
				Variable v2 = (Variable)right;
				if (v1.name.Equals(v2.name))
					return new Number(2) * v1;
			}
			// b + 3b
			else if (left.GetType() == typeof(Multip) && right.GetType() == typeof(Variable))
			{
				Variable v = (Variable)right;
				Multip m = (Multip)left;
				if (m.isMultipOfVariable(v))
				{
					Operation other = m.findOther(v);
					if (other.isNumber())
					{
						((Number)other).val += 1;
						return m;
					}
				}
			}
			else if (left.GetType() == typeof(Variable) && right.GetType() == typeof(Multip))
			{
				Variable v = (Variable)left;
				Multip m = (Multip) right;
				if (m.isMultipOfVariable(v))
				{
					Operation other = m.findOther(v);
					if (other.isNumber())
					{
						((Number)other).val += 1;
						return m;
					}
				}
			}
			// 2b+3b	2c+3d
			else if (left.GetType() == typeof(Multip) && right.GetType() == typeof(Multip))
			{
				var lmult = (Multip)left;
				var rmult = (Multip)right;
				List<Operation> lnans = lmult.getNaN();
				List<Operation> rnans = rmult.getNaN();
				if (lnans.Count==1 && rnans.Count==1)
				{
					if (lnans.First().GetType() == typeof(Variable) && rnans.First().GetType() == typeof(Variable))
					{
						var lv = (Variable)lnans.First();
						var rv = (Variable)rnans.First();
						if ((lmult).isMultipOfVariable(rv))
						{
							Number lnum = lmult.getNumbers().First();
							Number rnum = rmult.getNumbers().First();
							lnum.val += rnum.val;
							return lnum * rv;
						}
					}
					throw new NotImplementedException();
				}
			}
			this.left = left;
			this.right = right;
			return this;
            //throw new Exception("reached unreachable code!");
        }
        public Operation sum(Operation left, Operation right)
		{
			if (left.isNegative() && right.isPositive())
			{
				Operation tmp = left;
				left = right;
				right = tmp;
			}

            if (left.GetType() == typeof(Number))
                if (right.GetType() == typeof(Number))  // number + number
                    return sum((Number)left, (Number)right);
                else if (right.GetType() == typeof(Sum))// number + sum
                    return sum((Number)left, (Sum)right);
                else                                    // number + equation
                    return sumThis(left, right);
            else if (left.GetType() == typeof(Sum))
                if (right.GetType() == typeof(Number))  // sum + number
                    return sum((Number)right, (Sum)left);
                else if (right.GetType() == typeof(Sum))// sum + sum
                    return sum((Sum)left, (Sum)right);
                else                                    // sum + equation
                    return sumThis(left, right);
            else                                        // equation + equation
                return sumThis(left, right);
        }
		private Operation simplifySumOfMultip(Operation left, Operation right)
		{
			bool lm = left.GetType() == typeof(Multip);
			bool rm = right.GetType() == typeof(Multip);
			if (lm && rm)
			{
				List<Variable> vars = new List<Variable>();
				
			}
			
				
			return this;
		}
		public override string ToString()
		{
			if (right.isNegative())
				return left.ToString() + right.ToString();
			else
				return left.ToString() + '+' + right.ToString();
		}
	}

	class Multip : Operation
	{
		public Multip()
		{
			op = "*";
			//elements = new List<Equation>();
		}

		public override bool isOne()
		{
			return false;
		}
		public override bool isZero()
		{
			return left.isZero() || right.isZero();
		}
		public override bool isNegative()
		{
			return left.isNegative() ^ right.isNegative();
		}
		public override bool isPositive()
		{
			return left.isPositive() && right.isPositive() || left.isNegative() && right.isNegative();
		}
		public Operation multiply(Variable left, Variable right)
		{
			if (left.name.Equals(right.name))
				return new Power().power(left, new Number(2));
			else
			{
				this.left = left;
				this.right = right;
				return this;
			}
		}
		public Operation multiply(Number n, Variable v)
		{
			left = n;
			right = v;
			return this;
		}
		public Operation multiply(Operation left, Operation right)
		{
			if (left.isOne()) return right;
			if (right.isOne()) return left;
			if (left.isZero() || right.isZero()) return new Number(0);

			if (left.GetType() == typeof(Sum) && right.GetType() == typeof(Sum))
			{
				Operation o1, o2, o3, o4;
				o1 = left.left * right.left;
				o2 = left.left * right.right;
				o3 = left.right * right.left;
				o4 = left.right * right.right;
				return o1+o2+o3+o4;
			}
			else if (left.GetType() == typeof(Sum))
			{
				this.left = left.left * right;
				this.right = left.right * right;
				return this.left + this.right;
			}
			else if (right.GetType() == typeof(Sum))
			{
				this.left = left * right.left;
				this.right = left * right.right;
				return this.left + this.right;
			}

			List<Operation> factors = new List<Operation>();
			if (left.GetType() == typeof(Multip))
			{
				Multip m = (Multip)left;
				factors.Add(m.left);
				factors.Add(m.right);
			}
			else
				factors.Add(left);

			if (right.GetType() == typeof(Multip))
			{
				Multip m = (Multip)right;
				factors.Add(m.left);
				factors.Add(m.right);
			}
			else
				factors.Add(right);

			List<Number> numbers = factors.getNumbers();
			double val = 1.0;
			foreach(Number n in numbers)
				val *= n.val;
			this.left = new Number(val);

			List<Operation> nans = factors.getNaN();
			if (nans.Count == 0)
				return this.left;
			else if (nans.Count == 1)
				this.right = nans[0];
			else if (nans.Count == 2 && numbers.Count == 0)
			{
				this.left = nans[0];
				this.right = nans[1];
				return this;
			}
			else if (nans.Count == 2 && numbers.Count > 0)
			{
			//	if (nans[0].GetType() == typeof(Variable) && nans[1].GetType() == typeof(Variable))
			//	{
					this.right = multiply((Variable)nans[0], (Variable)nans[1]);
					return this;
			//	}
			//	else
			//		throw new NotImplementedException();
			}
			else if (nans.Count > 2)
			{
				throw new NotImplementedException("multiply: nans.Count>2");
			}
			
			return this;
		}
		public bool find(Variable v)
		{
			if (left.isVaribale() && ((Variable)left).name.Equals(v.name))
				return true;
			if (right.isVaribale() && ((Variable)right).name.Equals(v.name))
				return true;
			if (left.GetType() == typeof(Multip) && ((Multip)left).find(v))
				return true;
			if (right.GetType() == typeof(Multip) && ((Multip)right).find(v))
				return true;
			return false;
		}
		public Operation findOther(Variable v)
		{
			Operation ret = null;
			if (left.isVaribale() && ((Variable)left).name.Equals(v.name))
				return right;

			if (right.isVaribale() && ((Variable)right).name.Equals(v.name))
				return left;
			/*
			if (left.GetType() == typeof(Multip))
				ret = ((Multip)left).findOther(v);
				if (!Object.ReferenceEquals(null, ret))
					return ret;

			if (right.GetType() == typeof(Multip))
				ret = ((Multip)right).findOther(v);
				if (!Object.ReferenceEquals(null, ret))
					return ret;*/
			return ret;
		}
		public bool isMultipOfVariable(Variable v)
		{
			if (left.GetType() == typeof(Number) && right.GetType() == typeof(Variable) && ((Variable)right).name.Equals(v.name))
				return true;
			else if (right.GetType() == typeof(Number) && left.GetType() == typeof(Variable) && ((Variable)left).name.Equals(v.name))
			{
				Variable tmp = (Variable)left;
				left = right;
				right = tmp;
				return true;
			}
			return false;
		}
		public override Operation derivative(string x)
		{
			//TODO multip derivative
			throw new NotImplementedException();
		}
		public override string ToString()
		{
			bool leftbrackets = left.GetType()!=typeof(Number) && left.GetType()!=typeof(Variable) && left.GetType()!=typeof(Power);
			bool rightbrackets = right.GetType()!=typeof(Number) && right.GetType()!=typeof(Variable) && right.GetType()!=typeof(Power);
			bool lpow = left.GetType() == typeof(Power);
			bool lpowrnum = lpow && left.right.GetType() == typeof(Number);
			bool lnum = left.GetType() == typeof(Number);
			bool ope = !lpowrnum && !lnum || !lpow && lpowrnum && !lnum;
			StringBuilder ret = new StringBuilder();
			
			if (leftbrackets) ret.Append('(');
			ret.Append(left.ToString());
			if (leftbrackets) ret.Append(')');
			if (ope) ret.Append(op);
			if (rightbrackets) ret.Append('(');
			ret.Append(right.ToString());
			if (rightbrackets) ret.Append(')');
			
			return ret.ToString();
		}
	}

    class Power : Operation
    {
        public Power()
		{
			op = "^";
		}

		public override bool isOne()
		{
            if (left.isOne())
                return true;
            else if (!left.isZero())
                return right.isZero();
			return false;
		}
		public override bool isZero()
		{
			return false;
		}
		public override bool isNegative()
		{
			if (left.isPositive())
				return true;
			else if ((!left.isNegative() || left.isPositive()) && right.isNegative())
				return true;
			else if (right.isNumber() && Math.Floor(((Number)right).val) != ((Number)right).val)
				throw new NotImplementedException("real number");
			throw new NotImplementedException();
		}
		public override bool isPositive()
		{
			return left.isPositive(); // || right.dval % 2 == 0;
		}
		public Operation power(Operation left, Operation right)
		{
			if (right.isZero() || left.isOne())
				return new Number(1);
			if (left.isZero() && right.isZero())
				throw new Exception("math error: 0^0");

			if (left.GetType() == typeof(Minus)
			&& right.GetType() == typeof(Number)
			&& ((Number)right).val % 2 == 0.0)
			{
				left = left.left;
			}
            if (left.GetType() == typeof(Number) && right.GetType() == typeof(Number))
            {
                double l = ((Number)left).val;
                double r = ((Number)right).val;
                return new Number(Math.Pow(l, r));
            }
			else if (left.GetType() == typeof(Multip) && right.GetType() == typeof(Number))
			{
				Multip m = (Multip)left;
				Number n = (Number)right;
				return (m.left ^ n) * (m.right ^ n);
			}
            else
            {
                this.left = left;
                this.right = right;
                return this;
            }
            //else if (left.GetType() == typeof(Number))
            //    return makePower((Number)left, right);
            //else if (left.GetType() == typeof(Number))
            //    return makePower(left, (Number)right);
            //else
            //{
            //    this.left = left;
            //    this.right = right;
            //    return this;
            //}
		}
		public override Operation derivative(string x)
		{
            // TODO power derivative
            throw new NotImplementedException();
		}
		public override string ToString()
		{
			if ((left.isNumber() || left.isVaribale()) && (right.isNumber() || right.isVaribale()))
			{
				return left.ToString() + op + right.ToString();
			}
			else if (left.isNumber() || left.isVaribale())
			{
				return left.ToString() + op + '(' + right.ToString() + ')';
			}
			else if (right.isNumber() || right.isVaribale())
			{
				return '(' + left.ToString() + ')' + op + right.ToString();
			}
			else
				return '(' + left.ToString() + ')' + op + '(' + right.ToString() + ')';
		}

    }

	abstract class Function : Operation {}

	class Sin : Function
	{
		bool opp = false;
		Operation element;
		public Sin()
		{
			op = "sin";
		}

		public Operation sin(Operation e)
		{
			if (e.GetType() == typeof(Number))
				if (((Number)e).val % Math.PI == 0.0)
					return new Number(0);
				else
					return new Number(Math.Sin(((Number)e).val));
			else
				element = e;
			return this;
		}
		public override Operation derivative(string x)
		{
			return new Cos().cos(this);
		}
		public override Operation reciprocal()
		{
			rec = !rec;
			return this;
		}
		public override bool isOne()
		{
			return left == new Number(Math.PI);
		}
		public override bool isZero()
		{
			return left.isZero();
		}
		public override bool isNegative()
		{
			throw new NotImplementedException();
		}
		public override bool isPositive()
		{
			throw new NotImplementedException();
		}
		public override string ToString()
		{
			return (opp?"-":"") + (rec?"1/":"") + "sin("+left.ToString()+')';
		}
	}

	class Cos : Function
	{
		bool opp = false;
		Operation element;

		public Cos()
		{
			op = "cos";
		}
		public override Operation reciprocal()
		{
			rec = !rec;
			return this;
		}
		public Operation cos(Operation e)
		{
			if (e.GetType()==typeof(Number))
				if ((((Number)e).val + Math.PI/2) % Math.PI == 0.0)
					return new Number(0);
				else
					return new Number(Math.Cos(((Number)e).val));
			
			element = e;
			return this;
		}
		public override Operation derivative(string x)
		{
			return new Minus().minus(new Sin().sin(this));
		}
		public override bool isOne()
		{
			return false;
		}
		public override bool isZero()
		{
			return false;
		}
		public override bool isNegative()
		{
			throw new NotImplementedException();
		}
		public override bool isPositive()
		{
			throw new NotImplementedException();
		}
		public override string ToString()
		{
			return (opp ? "-" : "") + (rec ? "1/" : "") + "cos(" + left.ToString() + ')';
		}
	}

    class Tan : Function
    {
        bool opp = false;
        public Tan()
        {
            op = "tan";
        }
        public override Operation reciprocal()
        {
            rec = !rec;
            return this;
        }
        public Operation tan(Operation e)
        {
            if (e.GetType() == typeof(Number))
				if ((((Number)e).val + 0.5 * Math.PI) % Math.PI == 0.0)
					throw new Exception("math error: the value of tan is NaN");
				else if (((Number)e).val % Math.PI == 0.0)
					return new Number(0);
				else
					return new Number(Math.Tan(((Number)e).val));

            left = e;
            return this;
        }
        public override Operation derivative(string x)
        {
			return new Multip().multiply(new Number(1.0), new Sin().sin(left));
            //return new Sum().sum(new Number(1),new Power().power(this,new Number(2)));
        }
        public override bool isOne()
        {
            return false;
        }
        public override bool isZero()
        {
            return false;
        }
		public override bool isNegative()
		{
			throw new NotImplementedException();
		}
		public override bool isPositive()
		{
			throw new NotImplementedException();
		}
        public override string ToString()
        {
            return (opp ? "-" : "") + (rec ? "1/" : "") + "tan(" + left.ToString() + ')';
        }
    }

	class Ctg : Function
	{
		bool opp = false;

		public Ctg()
		{
			op = "ctg";
		}
		public override Operation reciprocal()
		{
			rec = !rec;
			return this;
		}
		public Operation ctg(Operation e)
		{
			if (e.GetType() == typeof(Number))
				if ((((Number)e).val + 0.5 * Math.PI) % Math.PI == 0.0)
					return new Number(0);
				else if (((Number)e).val % Math.PI == 0.0)
					throw new Exception("math error: the value of ctg is NaN");
				else
					return new Number(1/Math.Tan(((Number)e).val));

			left = e;
			return this;
		}
		public override Operation derivative(string x)
		{
			return new Sum().sum(new Number(1), new Power().power(this, new Number(2)));
		}
		public override bool isOne()
		{
			return false;
		}
		public override bool isZero()
		{
			return false;
		}
		public override bool isNegative()
		{
			throw new NotImplementedException();
		}
		public override bool isPositive()
		{
			throw new NotImplementedException();
		}
		public override string ToString()
		{
			return (opp ? "-" : "") + (rec ? "1/" : "") + "ctg(" + left.ToString() + ')';
		}
	}
	class Factorial : Operation
	{
		public Factorial()
		{
			op = "!";
		}
		public override bool isZero()
		{
			throw new NotImplementedException();
		}
		public override bool isOne()
		{
			throw new NotImplementedException();
		}
		public override Operation derivative(string x)
		{
			throw new NotImplementedException();
		}
		public override bool isNegative()
		{
			throw new NotImplementedException();
		}
		public override bool isPositive()
		{
			throw new NotImplementedException();
		}
		public Operation factorial(Operation o)
		{
			left = o;
			return this;
		}
		public override string ToString()
		{
			// TODO: brackets
			return '('+left.ToString() +')'+ op;
		}
	}
	public abstract class Value : Operation { }
	public class Number : Value
	{
		public double val;
		public Number(double val) {
			this.val=val;
		}
		public override Operation reciprocal()
		{
			val = 1/val;
			return this;
		}
		public override bool isOne() { return val==1.0 ? true : false; }
		public override bool isZero() { return val==0.0 ? true : false; }
		public override bool isNegative() { return val<0?true:false; }
		public override bool isPositive() { return val>=0?true:false; }
		public override bool isNumber() { return true; }
		public override bool isVaribale() { return false; }
		public override Operation derivative(string x) { return new Number(0); }
		public override string toTree(string tabs = "")
		{
			return tabs + val.ToString();
		}
		public override string ToString()
		{
			//TODO wyświetlanie ułamków: można zrobić 1/xxx zamiast 0.xxx
			return string.Format("{0:0.#####}", val);
		}
	}
	class Variable : Value
	{
		public string name { get; private set; }
		private bool opp = false;

		public Variable(string s) {
			name=s;
		}
		public override Operation reciprocal()
		{
			rec = !rec;
			return this;
		}
		public override bool isOne() { return false; }
		public override bool isZero() { return false; }
		public override bool isNegative()
		{
			return false;
		}
		public override bool isPositive()
		{
			return true;
		}
		public override bool isVaribale() { return true; }
		public override bool isNumber() { return false; }
		public override Operation derivative(string x)
		{
			if (name.Equals(x))
				return new Number(1);
			else
				return new Number(0);
		}
		public override string toTree(string tabs = "")
		{
			return tabs + name;
		}

		public override string ToString()
		{
			return (rec?"1/":"")+name;
		}
	}
	class Calculation
	{
		string			input;
		TokenStream		ts;
		Operation		result;
		List<String>	functions;
		const string	error_text = "error";

		Dictionary<string, double> variables;

		public Calculation(String c)
		{
			input = c;
			ts = new TokenStream(input);
			
            functions = new List<string>();
			functions.Add("sin");
			functions.Add("cos");
            functions.Add("tan");
            functions.Add("ctg");

			variables = new Dictionary<string,double>();
			variables.Add("pi", Math.PI);
            variables.Add("euler", Math.E);
		}

		private bool isFunction(string func)
		{
			func = func.ToLower();
			foreach(string s in functions)
				if (s.Equals(func))
					return true;
			return false;
		}

		private bool isVariable(string var)
		{
			return variables.ContainsKey(var.ToLower());
		}

		private Operation calculateFunction(string f, Operation e)
		{
			switch (f.ToLower())
			{
				case "sin": return new Sin().sin(e);
				case "cos": return new Cos().cos(e);
                case "tan": return new Tan().tan(e);
				case "ctg": return new Ctg().ctg(e);
				//TODO więcej funkcji
			}
			throw new Exception("no function: "+f);
		}
		private Operation factorial(Operation o)
		{
			if (o.GetType() == typeof(Number) && (int)((Number)o).val==((Number)o).val)
				return new Number(factorial((int)((Number)o).val));
			return new Factorial().factorial(o);
		}
		private int factorial(int n)
		{
			if (n<0) throw new Exception("math error: number<0");
			if (n==0 || n==1) return 1;
			int ret = 1;
			for (int i=1; i<=n; i++)
				ret *= i;
			return ret;
		}
		delegate Operation level();
		private Operation name()
		{
			Token func = ts.get();
			if (!func.isName())
				throw new Exception("name expected");
			#if FUNCTIONS
			if (isFunction(func.sval))
			{
				Token t2 = ts.get();
				if (t2.kind != Token.BRACKETL) throw new Exception("'(' expected");
				//Equation e = expression();
				Operation e = calculateFunction(func.sval, expression());
				t2 = ts.get();
				if (t2.kind != Token.BRACKETR) throw new Exception("')' expected");
				return e;
			}
			else
			#endif
			if (isVariable(func.sval))
				return new Number(variables[func.sval.ToLower()]);
			else
			{
				//variables.Add(func.sval, null);
				return func.toValue();
			}
		}
		private Operation primary()
		{
			Token t = ts.get();
			if (t.isBracketL())
			{
				Operation e = expression();
				t = ts.get();
				if (t.kind != Token.BRACKETR) throw new Exception("')' expected");
				return e;
			}
			else if (t.isMinus())
				return -primary();
			else if (t.isPlus())
				return primary();
			else if (t.isNumber())
				return t.toValue();
			else if (t.isName()) {
				ts.putback(t);
				return name();
				//return t.toValue();
			}
			else 
				throw new Exception("factor expected");
		}
		private Operation postOperator()
		{
			level lower = primary;
			Operation left = lower();
			Token op = ts.get();
			switch (op.kind)
			{
				#if DERIVATIVES
				case Token.DERIVATIVE:
					return left.derivative("x");
				#endif
				#if FACTORIALS
				case Token.FACTORIAL:
					return factorial(left);
				#endif
				default:
					ts.putback(op);
					break;
			}
			return left;
		}
		private Operation exp()
		{
			level lower = postOperator;
			Operation left = lower();
			Token op = ts.get();
			while (true)
			{
				switch (op.kind)
				{
				case Token.EXPONENTIATION:
					left ^= lower();
					op = ts.get();
					break;
				default:
					ts.putback(op);
					return left;
				}
			}
		}
		private Operation term()
		{
			level lower = exp;
			Operation left = lower();
			Token t = ts.get();
			while (true)
			{
				switch (t.kind)
				{
				case Token.MULTIPLICATION:
					left *= lower();
					t = ts.get();
					break;

				case Token.NAME: case Token.NUMBER:
					ts.putback(t);
					left *= lower();
					t = ts.get();
					break;

				case Token.DIVISION:
					{
						Operation e = lower();
						if (e.isZero()) throw new Exception("divide by zero");
						left /= e;
						t = ts.get();
						break;
					}
				default:
					ts.putback(t);
					return left;
				}
			}
		}
		private Operation expression()
		{
			level lower = term;
			Operation left = lower();
			Token op = ts.get();
			
			while (true) {
				switch (op.kind)
				{
					case '+':
						left += lower();
						op = ts.get();
						break;
					case '-':
						left -= lower();
						op = ts.get();
						break;
					default:
						ts.putback(op);
						return left;
				}
			}
		}
		public void calculate()
		{
			result = expression();
			Token t = ts.get();
			if (t.kind != Token.END)
				throw new InvalidTokenException(t.kind.ToString());
		}
		public string getResult()
		{
			return result.ToString();
		}
	}
}
