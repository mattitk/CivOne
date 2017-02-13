// CivOne
//
// To the extent possible under law, the person who associated CC0 with
// CivOne has waived all copyright and related or neighboring rights
// to CivOne.
//
// You should have received a copy of the CC0 legalcode along with this
// work. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.

using System.Linq;
using CivOne.Enums;
using CivOne.Events;
using CivOne.GFX;
using CivOne.Interfaces;
using CivOne.Templates;

namespace CivOne.Screens
{
	internal class CityInfo : BaseScreen
	{
		private readonly City _city;

		private readonly Picture _background;
		
		private CityInfoChoice _choice = CityInfoChoice.Info;
		private bool _update = true;

		private Picture InfoFrame
		{
			get
			{
				Picture output = new Picture(144, 83);
				IUnit[] units = _city.Tile.Units;
				for (int i = 0; i < units.Length; i++)
				{
					int xx = 4 + ((i % 6) * 18);
					int yy = 0 + (((i - (i % 6)) / 6) * 16);

					output.AddLayer(units[i].GetUnit(units[i].Owner), xx, yy);
					string homeCity = "NON.";
					if (units[i].Home != null)
						homeCity = $"{units[i].Home.Name.Substring(0, 3)}.";
					output.DrawText(homeCity, 1, 5, xx, yy + 16);
				}
				return output;
			}
		}

		private Picture HappyFrame
		{
			get
			{
				//TODO: Draw happiness data/stats
				Picture output = new Picture(144, 83);
				output.FillRectangle(1, 5, 15, 122, 1);
				output.FillRectangle(1, 5, 31, 122, 1);
				
				for (int yy = 1; yy < 30; yy+= 16)
				for (int i = 0; i < _city.Size; i++)
				{
					if (i < _city.ResourceTiles.Count() - 1)
					{
						output.AddLayer(Icons.Citizen((i % 2 == 0) ? Citizen.ContentMale : Citizen.ContentFemale), 7 + (8 * i), yy);
						continue;
					}
					output.AddLayer(Icons.Citizen(Citizen.Entertainer), 7 + (8 * i), yy);
				}
				return output;
			}
		}
		
		private Picture MapFrame
		{
			get
			{
				//TODO: Draw map
				Picture output = new Picture(144, 83);
				output.FillRectangle(9, 5, 2, 122, 1);
				output.FillRectangle(9, 5, 3, 1, 74);
				output.FillRectangle(9, 126, 3, 1, 74);
				output.FillRectangle(9, 5, 77, 122, 1);
				output.FillRectangle(5, 6, 3, 120, 74);
				return output;
			}
		}

		public override bool HasUpdate(uint gameTick)
		{
			if (_update)
			{
				_canvas.FillLayerTile(_background);
				_canvas.AddBorder(1, 1, 0, 0, 133, 92);
				_canvas.FillRectangle(0, 133, 0, 3, 92);
				
				DrawButton("Info", (byte)((_choice == CityInfoChoice.Info) ? 15 : 9), 1, 0, 0, 34);
				DrawButton("Happy", (byte)((_choice == CityInfoChoice.Happy) ? 15 : 9), 1, 34, 0, 32);
				DrawButton("Map", (byte)((_choice == CityInfoChoice.Map) ? 15 : 9), 1, 66, 0, 33);
				DrawButton("View", 9, 1, 99, 0, 33);

				switch (_choice)
				{
					case CityInfoChoice.Info:
						AddLayer(InfoFrame, 0, 9);
						break;
					case CityInfoChoice.Happy:
						AddLayer(HappyFrame, 0, 9);
						break;
					case CityInfoChoice.Map:
						AddLayer(MapFrame, 0, 9);
						break;
				}

				_update = false;
			}
			return true;
		}

		private bool GotoInfo()
		{
			_choice = CityInfoChoice.Info;
			_update = true;
			return true;
		}

		private bool GotoHappy()
		{
			_choice = CityInfoChoice.Happy;
			_update = true;
			return true;
		}

		private bool GotoMap()
		{
			_choice = CityInfoChoice.Map;
			_update = true;
			return true;
		}

		private bool GotoView()
		{
			_choice = CityInfoChoice.Info;
			_update = true;
			Common.AddScreen(new CityView(_city));
			return true;
		}
		
		public override bool KeyDown(KeyboardEventArgs args)
		{
			switch (args.KeyChar)
			{
				case 'I':
					return GotoInfo();
				case 'H':
					return GotoHappy();
				case 'M':
					return GotoMap();
				case 'V':
					return GotoView();
			}
			return false;
		}

		private bool InfoClick(ScreenEventArgs args)
		{
			IUnit[] units = _city.Tile.Units;
			for (int i = 0; i < units.Length; i++)
			{
				int xx = 4 + ((i % 6) * 18);
				int yy = 0 + (((i - (i % 6)) / 6) * 16);

				if (new Rectangle(xx, yy, 16, 16).Contains(args.Location))
				{
					units[i].Busy = false;
					Game.ActiveUnit = units[i];
					_update = true;
					break;
				}
			}
			return true;
		}
		
		public override bool MouseDown(ScreenEventArgs args)
		{
			if (args.Y < 10)
			{
				if (args.X < 34) return GotoInfo();
				else if (args.X < 66) return GotoHappy();
				else if (args.X < 99) return GotoMap();
				else if (args.X < 132) return GotoView();
			}
			
			switch (_choice)
			{
				case CityInfoChoice.Info:
					MouseArgsOffset(ref args, 0, 9);
					return InfoClick(args);
				case CityInfoChoice.Happy:
				case CityInfoChoice.Map:
					break;
			}
			return true;
		}

		public void Close()
		{
			Destroy();
		}

		public CityInfo(City city, Picture background)
		{
			_city = city;
			_background = background;

			_canvas = new Picture(136, 92, background.Palette);
		}
	}
}