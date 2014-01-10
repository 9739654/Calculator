using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculator
{

	public static class QueueExtension
	{
		public static char Peek(this Queue<char> q, char c)
		{
			return q.Count() > 0 ? q.Peek() : c;
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

	class Token
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

		private char	kind;
		private double	dval = 0.0;
		private string	sval = "";

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
			// TODO: wykrywanie tokena... gotowe?
			readToken(queue);
		}
		public	char		getKind()
		{
			return kind;
		}
		public	double	getD()
		{
			return dval;
		}
		public	string	getS()
		{
			return sval;
		}
		public	void	setVal(String val)
		{
			sval = val;
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
		private void	readToken(Queue<char> q)
		{
			char c = q.Peek(END);
			if (isDigit(c))
			{
				bool b = readNumber(q, out dval);
				if (b)
					kind = NUMBER;
				else
				{
					dval = 0;
					kind = ERROR;
					sval = "ERROR: readNumber(q, out d);";
					return;
				}
			}
			else if (isLetter(c))
			{
				bool b = readName(q, out sval);
				if (b)
					kind = NAME;
				else
				{
					kind = ERROR;
					sval = "nierozpoznany string";
					return;
				}
			}
			else if (c == END)
				kind = END;
			else if (c == DERIVATIVE)
				kind = DERIVATIVE;
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
					
					default:
						kind = ERROR;
						sval = string.Format("ERROR: nieznany znak: %c", c);
						break;
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
			if (!c.Equals('.'))
			{
				d = i;
				return true;
			}
			else
			{
				c = q.Dequeue();	// kropka, która była tylko .Peek()
				c = q.Peek(END);
				if (!isDigit(c))
				{
					// TODO: błąd po kropce musi być część ułamkowa
					d = 0.0;
					return false;
				}
				else
				{
					d = i + readDec(q);
					return true;
				}

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
			while (isLetter(c) || isDigit(c))
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
				// TODO: tu nie można dojść chyba, że błąd
				return false;
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
		public	override string ToString()
		{
			if (kind == NUMBER) return dval.ToString();

			if (kind == NAME || kind == ERROR) return sval;

			return kind.ToString();
		}
		public	static	Token operator +(Token t1, Token t2)
		{
			if (t1.getKind() == NUMBER && t2.getKind() == NUMBER)
			{
				return new Token(t1.getD() + t2.getD());
			}
			return new Token();
		}
		public	static	Token operator -(Token t1, Token t2)
		{
			if (t1.getKind() == NUMBER && t2.getKind() == NUMBER)
			{
				return new Token(t1.getD() - t2.getD());
			}
			return new Token();
		}
		public	static	Token operator *(Token t1, Token t2)
		{
			if (t1.getKind() == NUMBER && t2.getKind() == NUMBER)
			{
				return new Token(t1.getD() * t2.getD());
			}
			return new Token();
		}
		public	static	Token operator /(Token t1, Token t2)
		{
			if (t1.getKind() == NUMBER && t2.getKind() == NUMBER)
			{
				if (t2.getD() == 0.0)
				{
					return new Token(ERROR, "Dzielenie przez zero!");
				}
				return new Token(t1.getD() / t2.getD());
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
				if (t.getKind() == Token.END)
					finished = true;
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

	abstract class Equation
	{
		public virtual List<Equation> elements { get; set; }
		public string op = "";

		public virtual Equation opposite()			// returns the opposite
		{
			foreach(Equation e in elements)
				e.opposite();
			return this;
		}
		public virtual Equation reciprocal()			// returns the reciprocal
		{
			foreach(Equation e in elements)
				e.reciprocal();
			return this;
		}

		public abstract bool isZero();					// true if zero otherwise false
		public abstract bool isOne();					// true if one otherwise false
		public virtual List<Number> getNumbers()
		{
			var r = new List<Number>();
			foreach(Equation e in elements)
				if (e.isNumber())
					r.Add((Number)e);
			return r;
		}
		public virtual List<Equation> getNaN()
		{
			var r = new List<Equation>();
			foreach (Equation e in elements)
				if (!e.isNumber())
					r.Add(e);
			return r;
		}
		public virtual bool isNumber()
		{
			foreach(Equation e in elements)
				if (e.GetType() == typeof(Variable))
					return false;
			return true;
		}

		public virtual bool isVariable() { return false; }
		public abstract string getKind();
		public virtual string toTree(string tabs="") {
			string r = tabs + op + '\n';
			foreach (Equation e in elements)
				r += tabs + '\t' +	e.toTree(tabs+'\t') + '\n';
			return r+'\n';
		}

		public static Equation operator +(Equation e1, Equation e2)
		{
			return new Sum().sum(e1,e2);
		}
		public static Equation operator -(Equation e)
		{
			return e.opposite();
		}
		public static Equation operator -(Equation e1, Equation e2)
		{
			return new Sum().sum(e1,e2.opposite());
		}
		public static Equation operator *(Equation e1, Equation e2)
		{
			return new Multip().multiply(e1, e2);
		}
		public static Equation operator /(Equation e1, Equation e2)
		{
			return new Multip().multiply(e1,e2.reciprocal());
		}
		public static Equation operator !(Equation e)
		{
			return e.reciprocal();
		}
		public static bool operator ==(Equation e1, Equation e2)
		{
			// TODO
			return false;
		}
		public static bool operator !=(Equation e1, Equation e2)
		{
			//TODO
			return true;
		}

		public abstract Equation derivative(string x);	// calculates the derivative

		public override bool Equals(object obj)
		{
			//TODO
			return false;
		}
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
		public override string ToString()
		{
			if (elements.Count() > 1)
			{
				string r = elements[0].ToString();
				for (int i = 1; i < elements.Count(); ++i)
				{
					r += op + elements[i].ToString();
				}
				return r;
			}
			else if (elements.Count == 1)
			{
				return elements[0].ToString();
			}
			else
				return "";
		}
	}

	class Sum : Equation
	{
        Equation left, right;
        bool rec = false;
		public Sum()
		{
			op = "+";
			//elements = new List<Equation>();
		}

		public override Equation opposite()
		{
            left.opposite();
            right.opposite();
			return this;
		}
		public override Equation reciprocal()
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
		public override Equation derivative(string x)
		{
			//TODO
			return this;
		}
        private Equation sum(Number n, Sum s)
        {
            if (s.getNumbers().Count == 1)
            {
                s.getNumbers().First().val += n.val;
                return s;
            }
            else
                return sumThis(left, right);
        }
        private Equation sum(Number n1, Number n2)
        {
            return new Number(((Number)n1).val + ((Number)n2).val);
        }
        private Equation sum(Sum s1, Sum s2)
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
                    return new Variable("<zła liczba NaN?>");
            }
        }
        private Equation sumThis(Equation e1, Equation e2)
        {
            left = e1;
            right = e2;
            return this;
        }
        public Equation sum(Equation left, Equation right)
		{
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

		public override string getKind()
		{
	 		return "sum";
		}

	}

	class Multip : Equation
	{
        Equation left;
        Equation right;
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
			foreach(Equation x in elements)
				if (x.isZero()) return true;
			return false;
		}
		public Equation multiply(Equation left, Equation right)
		{
            if (left.GetType() == typeof(Number) && right.GetType() == typeof(Number))
                return new Number(((Number)left).val * ((Number)right).val);
            else
            {
                // TODO
                this.left = left;
                this.right = right;
                return this;
            }
		}
		public override Equation derivative(string x)
		{
			//TODO
			return this;
		}

		public override string getKind()
		{
			return "multip";
		}
	}

    class Power : Equation
    {
        Equation left, right;

        public Power()
		{
			op = "^";
			//elements = new List<Equation>();
		}

		public override bool isOne()
		{
            if (elements[0].isOne())//TODO
                return true;
            else if (!elements[0].isZero())
                foreach (Equation e in elements)
                    if (e.isZero())
                        return true;
			return false;
		}
		public override bool isZero()
		{
			return false;
		}

        private double pow(double x, double a)
        {
            if (x == 0) return 0;
            if (x == 1 || a == 0) return 1;
            a = Math.Abs(a);
            double r = 1;
            while (a > 0)
            {
                r *= x;
                --a;
            }
            return r;
        }
        private void powerNumber(Number n)
		{
			foreach (Equation e in elements)
				if (e.GetType() == typeof(Number))
				{
					((Number)e).val = pow(((Number)e).val, n.val);
					return;
				}
			elements.Add(n);
		}

		public Equation power(Equation left, Equation right)
		{
            if (left.GetType() == typeof(Number) && right.GetType() == typeof(Number))
            {
                double l = ((Number)left).val;
                double r = ((Number)right).val;
                return new Number(pow(l, r));
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
		public override Equation derivative(string x)
		{
            // TODO
            throw new NotImplementedException("pochodna potęgi");
		}

		public override string getKind()
		{
			return "power";
		}
    }

	abstract class Function : Equation {}

	class Sin : Function
	{
		bool opp = false;
		bool rec = false;
		Equation element;

		public Sin()
		{
			op = "sin";
		}

		public Equation sin(Equation e)
		{
			if (e.GetType() == typeof(Number))
				return new Number(Math.Sin(((Number)e).val));
			else
				element = e;
			return this;
		}
		public override Equation derivative(string x)
		{
			return new Cos().cos(this);
		}

		public override Equation opposite()
		{
			opp = !opp;
			return this;
		}
		public override Equation reciprocal()
		{
			rec = !rec;
			return this;
		}
		public override string getKind()
		{
			return "sin";
		}

		public override bool isOne()
		{
			return elements[0].isOne();
		}
		public override bool isZero()
		{
			return elements[0].isZero();
		}

		public override string ToString()
		{
			return (opp?"-":"") + (rec?"1/":"") + "sin("+elements[0].ToString()+')';
		}
	}

	class Cos : Function
	{
		bool opp = false;
		bool rec = false;
		Equation element;

		public Cos()
		{
			op = "cos";
		}

		public override Equation opposite()
		{
			opp = !opp;
			return this;
		}
		public override Equation reciprocal()
		{
			rec = !rec;
			return this;
		}
		public Equation cos(Equation e)
		{
			if (e.GetType()==typeof(Number))
				return new Number(Math.Sin(((Number)e).val));
			
			element = e;
			return this;
		}
		public override string getKind()
		{
			return "cos";
		}
		public override Equation derivative(string x)
		{
			return new Sin().sin(this).opposite();
		}
		public override bool isOne()
		{
			//TODO
			throw new NotImplementedException();
		}
		public override bool isZero()
		{
			//TODO
			throw new NotImplementedException();
		}
		public override string ToString()
		{
			return (opp ? "-" : "") + (rec ? "1/" : "") + "sin(" + elements[0].ToString() + ')';
		}
	}

    class Tan : Function
    {
        bool opp = false;
        bool rec = false;
        Equation element;

        public Tan()
        {
            op = "tan";
        }

        public override Equation opposite()
        {
            opp = !opp;
            return this;
        }
        public override Equation reciprocal()
        {
            rec = !rec;
            return this;
        }
        public Equation tan(Equation e)
        {
            if (e.GetType() == typeof(Number))
                return new Number(Math.Tan(((Number)e).val));

            element = e;
            return this;
        }
        public override string getKind()
        {
            return "tan";
        }
        public override Equation derivative(string x)
        {
            return new Sum().sum(new Number(1),new Power().power(this,new Number(2)));
        }
        public override bool isOne()
        {
            //TODO
            throw new NotImplementedException();
        }
        public override bool isZero()
        {
            //TODO
            throw new NotImplementedException();
        }
        public override string ToString()
        {
            return (opp ? "-" : "") + (rec ? "1/" : "") + "sin(" + elements[0].ToString() + ')';
        }
    }

	abstract class Value : Equation { }

	class Number : Value
	{
		public double val;
		public Number(double val) {
			this.val=val;
			elements = null;
		}

		public override Equation opposite()
		{
			val = -val;
			return this;
		}
		public override Equation reciprocal()
		{
			val = 1/val;
			return this;
		}
		public override bool isOne() { return val==1.0 ? true : false; }
		public override bool isZero() { return val==0.0 ? true : false; }
		public override bool isNumber() { return true; }
		public override Equation derivative(string x) { return new Number(0); }

		public override string getKind()
		{
			return "number";
		}
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
		private string name;
		private bool rec = false;
		private bool opp = false;

		public Variable(string s) {
			name=s;
			elements = null;
		}

		public override Equation opposite()
		{
			opp = !opp;
			return this;
		}
		public override Equation reciprocal()
		{
			rec = !rec;
			return this;
		}
		public override bool isOne() { return false; }
		public override bool isZero() { return false; }
		public override bool isVariable() { return true; }
		public override Equation derivative(string x)
		{
			//TODO czy dobrze? ? ?
			if (name.Equals(x))
				return new Number(1);
			else
				return new Number(0);
		}

		public override string getKind()
		{
			return "variable";
		}
		public override string toTree(string tabs = "")
		{
			return tabs + name;
		}

		public override string ToString()
		{
			return (opp?"-":"")+(rec?"1/":"")+name;
		}
	}

	class Calculation
	{
		string			input;
		TokenStream		ts;
		Equation		result;
		List<String>	functions;

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
			foreach(string s in functions)
				if (s.Equals(func))
					return true;
			return false;
		}

		private bool isVariable(string var)
		{
			return variables.ContainsKey(var);
		}

		private Equation calculateFunction(string f, Equation e)
		{
			switch (f)
			{
				case "sin": return new Sin().sin(e);
				case "cos": return new Cos().cos(e);
                case "tan": return new Tan().tan(e);
				//TODO więcej funkcji
			}
			throw new Exception("nie znalazłem funkcji: "+f);
		}

		private Equation function()
		{
			Token func = ts.get();
			if (!func.isName())
				throw new Exception("t musi być nazwą");
			if (isFunction(func.getS()))
			{
				Token t2 = ts.get();
				if (t2.getKind() != Token.BRACKETL) throw new Exception("oczekiwałem nawiasu za nazwą funkcji");
				//Equation e = expression();
				Equation e = calculateFunction(func.getS(), expression());
				t2 = ts.get();
				if (t2.getKind() != Token.BRACKETR) throw new Exception("oczekiwałem nawiasu zamykającego za argumentami funkcji");
				return e;
			}
			else if (isVariable(func.getS()))
				return new Number(variables[func.getS()]);
			else
				return func.toValue();
		}

		private Equation primary()
		{
			Token t = ts.get();
			if (t.isBracketL())
			{
				Equation e = expression();
				t = ts.get();
				if (t.getKind() != Token.BRACKETR) throw new Exception("Oczekiwałem ')'");
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
				return function();
				//return t.toValue();
			}
			else 
				throw new Exception("Oczekiwano czynnika");
		}

		private Equation postOperator()
		{
			Equation e = primary();
			//TODO check postoperators
			Token t = ts.get();
			if (t.getKind() == Token.DERIVATIVE)
				return e.derivative("x");
			else
				ts.putback(t);
			return e;
		}

		private Equation term()
		{
			Equation left = postOperator();
			Token t = ts.get();
			while (true)
			{
				switch (t.getKind())
				{
				case Token.MULTIPLICATION:
					left *= postOperator();
					t = ts.get();
					break;

				case Token.DIVISION:
					{
						Equation e = postOperator();
						if (e.isZero()) throw new Exception("Dzielenie przez zero");
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

		private Equation expression()
		{
			Equation left = term();
			Token op = ts.get();
			while (true) {
				switch (op.getKind())
				{
					case '+':
						left += term();
						op = ts.get();
						break;
					case '-':
						left -= term();
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

			var n1 = new Number(2);
			var sin = new Sin().sin(n1);
			result = sin;

			try
			{
				result = expression();
			} catch (Exception) {
				result = new Variable("Błąd");
			}
		}

		public string getResult()
		{
			return result.ToString();
		}
	}
}
