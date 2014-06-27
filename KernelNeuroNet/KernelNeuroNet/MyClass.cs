using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using MatrixOperations;
using BaseConstInit;

namespace KernelNeuroNet
{
	public class KernelNet
	{
 		float[][] revCnts, multTr;
		byte[][] memImages;
		int N, fN;
	
		public KernelNet(byte[][] images, int n, int fn, int bitn, char chr)
		{
			N = n * n;
			fN = fn * fn;
			byte[][] expImages = new byte[images.Length][];
			memImages = new byte[images.Length][];
			
			for (int i = 0; i < images.GetLength(0); i++)
			{
				memImages[i] = Sify(images[i], bitn);
				expImages[i] = DimUp(memImages[i], fN);
			}
				
			float[][] cnts = null;
			BCI.init<float>(ref cnts, images.Length, images.Length);
			
			for (int i = 0; i < images.Length; i++)
			{
				for (int j = 0; j < images.Length; j++)
				{
					cnts[i][j] = K(expImages[i], expImages[j]);
					Console.Write(cnts[i][j] + " ");
				}
				Console.WriteLine();
			}
			revCnts = MtrxOps.GetReverse(cnts);
			multTr = MtrxOps.GetTransp<float>(MtrxOps.GetMult(MtrxOps.GetTransp<byte>(memImages), revCnts));
		}
		
		public Tuple<float, char> Recognize(byte[] image, int bitn)
		{
			float[] z = new float[memImages.Length], x = ToFloat(Sify(image, bitn)), y = new float[N];
			
			for (int r = 0; r < 10; r++)
			{
				for (int i = 0; i < memImages.Length; i++)
					z[i] = K(memImages[i], x);
				y = MtrxOps.GetMult(z, multTr);
				x = SigmoidActFunc(y);
			}
			return Tuple.Create(0F, 'a');
		}
		
		byte[] DimUp(byte[] v, int fn)
		{
			if (fn < v.Length)
				throw new Exception("Ok, but I'll not do this :) New dimension is lower than native.");
			if (fn >= Math.Pow(2, v.Length))
				throw new Exception("Can't get so high dimension :)");
			return DoDimUp(v, ref fn);
		}
		
		byte[] DoDimUp(byte[] v, ref int n)
		{
			int fild = 0;
			byte[] res = new byte[Math.Min((int)(Math.Pow(2, v.Length) - 1), n)];
			v.CopyTo(res, 0);
			n -= v.Length;
			fild += v.Length;
			
			for (int i = 0; i < v.Length; i++)
			{
				byte[] buf = new byte[Math.Min(v.Length - i - 1, n)];
				for (int j = 0; j < buf.Length; j++)
					buf[j] = (byte)(v[i] * v[i + 1 + j]);
				byte[] rec = DoDimUp(buf, ref n);
				Array.Copy(rec, 0, res, fild, rec.Length);
				fild += rec.Length;
				if (n == 0)
					return res;
			}
			
			return res;
		}
		
		float[] SigmoidActFunc(float[] v)
		{
			float[] res = new float[v.Length];
			float expa;
			
			for (int i = 0; i < v.Length; i++)
			{
				expa = (float)Math.Exp(10 * (v[i] - 0.5));
				res[i] = expa / (float)(1 + expa);
			}
			
			return res;	
		}
		
		float K(byte[] v1, byte[] v2)
		{
			if (v1.Length != v2.Length)
				throw new Exception("Kernel can't apply to vectors different dimensions");
			
			float res = 0;
			for (int i = 0; i < v1.Length; i++)
				res += (float)(v1[i] * v2[i]);
				
			return res;
		}
		
		float K(byte[] v1, float[] v2)
		{
			if (v1.Length != v2.Length)
				throw new Exception("Kernel can't apply to vectors different dimensions");
			
			float res = 0;
			for (int i = 0; i < v1.Length; i++)
				res += (float)(v1[i] * v2[i]);
				
			return res;
		}
		
		byte[] Sify(byte[] image, int bitn)
		{
			byte[] res = new byte[image.Length / bitn];
			for (int i = 0; i < res.Length; i++)
				res[i] = (byte)((image[i * bitn] == 0) ? 0 : 1);
			return res;
		}
		
		float[] ToFloat(byte[] image)
		{
			float[] res = new float[image.Length];
			for (int i = 0; i < image.Length; i++)
				res[i] = (float)image[i];
			return res;
		}
	}
}

