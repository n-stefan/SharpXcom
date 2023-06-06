/*
 * Copyright 2010-2016 OpenXcom Developers.
 *
 * This file is part of OpenXcom.
 *
 * OpenXcom is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * OpenXcom is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with OpenXcom.  If not, see <http://www.gnu.org/licenses/>.
 */

namespace SharpXcom.Mod;

/**
 * Represents the contents of the Geoscape globe,
 * such as world polygons, polylines, etc.
 * @sa Globe
 */
internal class RuleGlobe
{
    List<Polygon> _polygons;
    List<Polyline> _polylines;
    Dictionary<int, Texture> _textures;

    /**
	 * Creates a blank ruleset for globe contents.
	 */
    internal RuleGlobe() { }

	/**
	 *
	 */
	~RuleGlobe()
	{
		_polygons.Clear();
		_polylines.Clear();
		_textures.Clear();
	}

    /**
     * Returns the list of polygons in the globe.
     * @return Pointer to the list of polygons.
     */
    internal List<Polygon> getPolygons() =>
        _polygons;

	/**
	 * Loads the globe from a YAML file.
	 * @param node YAML node.
	 */
	internal void load(YamlNode node)
	{
		if (node["data"] != null)
		{
			_polygons.Clear();
			loadDat(FileMap.getFilePath(node["data"].ToString()));
		}
		if (node["polygons"] != null)
		{
			_polygons.Clear();
			foreach (var item in ((YamlSequenceNode)node["polygons"]).Children)
			{
				Polygon polygon = new Polygon(3);
				polygon.load(item);
				_polygons.Add(polygon);
			}
		}
		if (node["polylines"] != null)
		{
			_polylines.Clear();
			foreach (var item in ((YamlSequenceNode)node["polylines"]).Children)
			{
				Polyline polyline = new Polyline(3);
				polyline.load(item);
				_polylines.Add(polyline);
			}
		}
		foreach (var item in ((YamlSequenceNode)node["textures"]).Children)
		{
			if (item["id"] != null)
			{
				int id = int.Parse(item["id"].ToString());
				Texture texture;
				if (_textures.TryGetValue(id, out var value))
				{
					texture = value;
				}
				else
				{
					texture = new Texture(id);
					_textures[id] = texture;
				}
				texture.load(item);
			}
			else if (item["delete"] != null)
			{
				int id = int.Parse(item["delete"].ToString());
				if (_textures.ContainsKey(id))
				{
					_textures.Remove(id);
				}
			}
		}

		Globe.COUNTRY_LABEL_COLOR = (byte)int.Parse(node["countryColor"].ToString());
		Globe.CITY_LABEL_COLOR = (byte)int.Parse(node["cityColor"].ToString());
		Globe.BASE_LABEL_COLOR = (byte)int.Parse(node["baseColor"].ToString());
		Globe.LINE_COLOR = (byte)int.Parse(node["lineColor"].ToString());
		if (node["oceanPalette"] != null)
		{
			Globe.OCEAN_COLOR = Palette.blockOffset((byte)int.Parse(node["oceanPalette"].ToString()));
		}
		Globe.OCEAN_SHADING = bool.Parse(node["oceanShading"].ToString());
	}

	/**
	 * Loads a series of map polar coordinates in X-Com format,
	 * converts them and stores them in a set of polygons.
	 * @param filename Filename of the DAT file.
	 * @sa http://www.ufopaedia.org/index.php?title=WORLD.DAT
	 */
	void loadDat(string filename)
	{
		try
		{
			// Load file
			using var mapFile = new FileStream(filename, FileMode.Open);

            short[] value = new short[10];
			byte[] buffer = new byte[value.Length * sizeof(short)];

            while (mapFile.Read(buffer, 0, buffer.Length) != 0)
			{
				Polygon poly;
				int points;

				for (int i = 0; i < 10; ++i)
				{
					value[i] = BitConverter.ToInt16(buffer, i * 2);
				}

				if (value[6] != -1)
				{
					points = 4;
				}
				else
				{
					points = 3;
				}
				poly = new Polygon(points);

				for (int i = 0, j = 0; i < points; ++i)
				{
					// Correct X-Com degrees and convert to radians
					double lonRad = Xcom2Rad(value[j++]);
					double latRad = Xcom2Rad(value[j++]);

					poly.setLongitude(i, lonRad);
					poly.setLatitude(i, latRad);
				}
				poly.setTexture(value[8]);

				_polygons.Add(poly);
			}

			if (mapFile.Position != mapFile.Length)
			{
				throw new Exception("Invalid globe map");
			}

			mapFile.Close();
		}
		catch (Exception)
		{
			throw new Exception(filename + " not found");
		}
	}

	/**
	 * Returns the rules for the specified texture.
	 * @param id Texture ID.
	 * @return Rules for the texture.
	 */
	internal Texture getTexture(int id) =>
		_textures.TryGetValue(id, out Texture texture) ? texture : null;
}
