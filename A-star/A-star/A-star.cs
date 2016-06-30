using System;
using System.Collections.Generic;
using System.Linq;

//G - koszt od poczatkowego (nieliniowy), H - koszt do konca (manhattan distance?), F-suma G i H
namespace A_star
{
	public static class GlobalVar
	{
		public const int GridWidth = 7; public const int GridHeight = 7;
		public const int StartX = 5; public const int StartY = 5;
		public const int TargetX = 1; public const int TargetY = 1;
	}
	public enum State { empty, open, closed, wall };
	class Program
	{
		static int currentX = GlobalVar.StartX, currentY = GlobalVar.StartY, neighborX, neighborY;
		static int[] neighborXs = new int[] { 0, 1, 1, 1, 0, -1, -1, -1 }, neighborYs = new int[] { 1, 1, 0, -1, -1, -1, 0, 1 };
		static List<Node> Open = new List<Node>();
		static List<Node> Closed = new List<Node>();
		static List<Node> Wall = new List<Node>();
		static List<Node> FinalPath = new List<Node>();
		static Node tempNode;
		static void Main(string[] args)
		{
			bool fresh = true;
			ConsoleColor initBackground = Console.BackgroundColor;
			//Console.SetWindowSize(GlobalVar.GridWidth * 2 + 1, GlobalVar.GridHeight * 2);
			Node[,] Grid = new Node[GlobalVar.GridWidth, GlobalVar.GridHeight];
			for (int i = GlobalVar.GridHeight - 1; i >= 0; i--)
				for (int j = 0; j < GlobalVar.GridWidth; j++)
				{
					Grid[j, i] = new Node(j, i); //i-wiersze
					Grid[j, i].x = j; Grid[j, i].y = i;
				}
			Wall.Add(Grid[1, 5]); Wall.Add(Grid[1, 4]); Wall.Add(Grid[2, 4]); Wall.Add(Grid[3, 4]); Wall.Add(Grid[4, 4]); Wall.Add(Grid[5, 4]); Wall.Add(Grid[5, 3]); Wall.Add(Grid[5, 2]); Wall.Add(Grid[5, 1]);
			//poczatek algorytmu
			Closed.Add(Grid[GlobalVar.StartX, GlobalVar.StartY]);
			do
			{
				neighborX = currentX + 0; neighborY = currentY + 1;
				for (int i = 0; i < 8; i++)
				{
					neighborX = currentX + neighborXs[i]; neighborY = currentY + neighborYs[i];
					if (neighborX < GlobalVar.GridWidth && neighborY < GlobalVar.GridHeight && neighborX >= 0 && neighborY >= 0 &&
						!Closed.Contains(Grid[neighborX, neighborY]) && !Wall.Contains(Grid[neighborX, neighborY]))
					{
						if (fresh = !Open.Contains(Grid[neighborX, neighborY])) Open.Add(Grid[neighborX, neighborY]); //dodaje do open jesli jeszcze go tam nie ma
						Grid[neighborX, neighborY].CalculateGHF(Grid[currentX, currentY], fresh);
					}
				}
				if (!Open.Any()) {Console.WriteLine("No path!"); return;}
				Node Lowest = LowestCost(Open);
				Closed.Add(Lowest);
				currentX = Lowest.x; currentY = Lowest.y;
				Open.Remove(Lowest);
			
				//rysowanie
				tempNode = Grid[GlobalVar.TargetX, GlobalVar.TargetY];
				if (Closed.Contains(Grid[GlobalVar.TargetX,GlobalVar.TargetY])) //ostateczna sciezka
					do
					{
						FinalPath.Add(tempNode);
						tempNode = tempNode.parent;
					} while (tempNode.x != GlobalVar.StartX || tempNode.y != GlobalVar.StartY);
				Console.SetCursorPosition(0, 0);
				for (int i = GlobalVar.GridHeight - 1; i >= 0; i--)
					for (int j = 0; j < GlobalVar.GridWidth; j++)
					{
						Console.BackgroundColor = ConsoleColor.White;
						if (Open.Contains(Grid[j, i])) Console.BackgroundColor = ConsoleColor.Green;
						else if (Closed.Contains(Grid[j, i])) Console.BackgroundColor = ConsoleColor.Red;
						else if (Wall.Contains(Grid[j, i])) Console.BackgroundColor = ConsoleColor.Gray;
						if (j == GlobalVar.StartX && i == GlobalVar.StartY) Console.BackgroundColor = ConsoleColor.Yellow;
						if (j == GlobalVar.TargetX && i == GlobalVar.TargetY) Console.BackgroundColor = ConsoleColor.DarkMagenta;
						if (FinalPath.Contains(Grid[j, i])) Console.BackgroundColor = ConsoleColor.Cyan;
						Console.Write("{0,-2}", Grid[j, i].F != 0 ? Grid[j, i].F.ToString().Substring(0,2) : ""); //wyswietlanie samego tla, if to nowa linia
						if (j == GlobalVar.GridWidth - 1) Console.Write(Environment.NewLine);
					}
				Console.BackgroundColor = initBackground; Console.Write(Environment.NewLine);
				System.Threading.Thread.Sleep(400);
			} while (!Closed.Contains(Grid[GlobalVar.TargetX, GlobalVar.TargetY]));			
			Console.WriteLine("FINAL PATH COUNT: {0}", FinalPath.Count);
			Console.ReadKey();
		}
		static Node LowestCost(List<Node> NodeList)
		{
			int count = NodeList.Count;
			Node Min = NodeList[0];
			for (int i = 0; i < count; i++)
				if (NodeList[i].F < Min.F) Min = NodeList[i]; //najmniejszy F
																			 //znalezc duplikaty wzgl F i wybrac ten z najnizszym H
			List<Node> duplicateCosts = new List<Node>(Open);
			duplicateCosts.RemoveAll(node => node.F != Min.F); //usuwa rozne od Min
			if (duplicateCosts.Count > 1)
			{
				Node MinH = duplicateCosts[0];
				for (int i = 0; i < duplicateCosts.Count; i++)
					if (duplicateCosts[i].H < MinH.H) MinH = duplicateCosts[i];
				return MinH;
			}
			return Min;
		}
	}
	class Node
	{
		public int x, y;
		public Node parent;
		public int G, H, F;
		public State NodeState { get; private set; } = State.empty;
		public Node() { }
		public Node(int x, int y)
		{
			this.x = x;
			this.y = y;
		}
		public void CalculateGHF(Node newParent, bool isFresh)
		{	//jesli nowy G mniejszy od obecnego zaktualizuj
			int newG = newParent.G + (Math.Abs(this.x - newParent.x) + Math.Abs(this.y - newParent.y) == 2 ? 14 : 10);
			if (!isFresh && newG >= this.G) return;
			G = newG;
			this.parent = newParent;
			H = (Math.Abs(GlobalVar.TargetX - this.x) + Math.Abs(GlobalVar.TargetY - this.y)) * 10; //manhattan distance
			F = G + H;
			//Console.WriteLine("G: {0} H: {1} F:{2}", G, H, F); //test, chyba dobrze
		}
	}
}