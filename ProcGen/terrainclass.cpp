////////////////////////////////////////////////////////////////////////////////
// Filename: terrainclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "terrainclass.h"
#include <cmath>


TerrainClass::TerrainClass()
{
	m_vertexBuffer = 0;
	m_indexBuffer = 0;
	m_heightMap = 0;
	m_terrainGeneratedToggle = false;

	grassTexture = 0;
	slopeTexture = 0;
	rockTexture = 0;
	snowTexture = 0;
	sandTexture = 0;

	//seed random generator
	srand(time(NULL));
}


TerrainClass::TerrainClass(const TerrainClass& other)
{
}


TerrainClass::~TerrainClass()
{
}

bool TerrainClass::InitializeTerrain(ID3D11Device* device, int terrainWidth, int terrainHeight, WCHAR* grassFileName, WCHAR* slopeFileName, WCHAR* rockFileName, WCHAR* snowFileName, WCHAR* sandFileName)
{
	//init perlin noise
	perlinFuncs.Init();

	// Load the textures.
	grassTexture = new TextureClass;
	grassTexture->Initialize(device, grassFileName);

	slopeTexture = new TextureClass;
	slopeTexture->Initialize(device, slopeFileName);

	rockTexture = new TextureClass;
	rockTexture->Initialize(device, rockFileName);

	snowTexture = new TextureClass;
	snowTexture->Initialize(device, snowFileName);

	sandTexture = new TextureClass;
	sandTexture->Initialize(device, sandFileName);


	int index;
	float height = 0.0;
	bool result;

	// Save the dimensions of the terrain.
	m_terrainWidth = terrainWidth;
	m_terrainHeight = terrainHeight;

	// Create the structure to hold the terrain data.
	m_heightMap = new HeightMapType[m_terrainWidth * m_terrainHeight];
	if(!m_heightMap)
	{
		return false;
	}

	// Initialise the data in the height map (flat).
	for(int j=0; j<m_terrainHeight; j++)
	{
		for(int i=0; i<m_terrainWidth; i++)
		{			
			index = (m_terrainHeight * j) + i;

			m_heightMap[index].x = (float)i;
			m_heightMap[index].y = (float)height;
			m_heightMap[index].z = (float)j;

		}
	}


	//even though we are generating a flat terrain, we still need to normalise it. 
	// Calculate the normals for the terrain data.
	result = CalculateNormals();
	if(!result)
	{
		return false;
	}

	//redo texture coordinates
	CalculateTextureCoordinates();

	// Initialize the vertex and index buffer that hold the geometry for the terrain.
	result = InitializeBuffers(device);
	if(!result)
	{
		return false;
	}

	return true;
}
bool TerrainClass::Initialize(ID3D11Device* device, char* heightMapFilename)
{
	bool result;


	// Load in the height map for the terrain.
	result = LoadHeightMap(heightMapFilename);
	if(!result)
	{
		return false;
	}

	// Normalize the height of the height map.
	NormalizeHeightMap();

	// Calculate the normals for the terrain data.
	result = CalculateNormals();
	if(!result)
	{
		return false;
	}

	// Calculate the texture coordinates.
	CalculateTextureCoordinates();

	// Initialize the vertex and index buffer that hold the geometry for the terrain.
	result = InitializeBuffers(device);
	if(!result)
	{
		return false;
	}

	return true;
}


void TerrainClass::Shutdown()
{
	//release the textures
	if (grassTexture)
	{
		grassTexture->Shutdown();
		delete grassTexture;
		grassTexture = 0;
	}
	if (slopeTexture)
	{
		slopeTexture->Shutdown();
		delete slopeTexture;
		slopeTexture = 0;
	}
	if (rockTexture)
	{
		rockTexture->Shutdown();
		delete rockTexture;
		rockTexture = 0;
	}
	if (snowTexture)
	{
		snowTexture->Shutdown();
		delete snowTexture;
		snowTexture = 0;
	}
	if (sandTexture)
	{
		sandTexture->Shutdown();
		delete sandTexture;
		sandTexture = 0;
	}

	// Release the vertex and index buffer.
	ShutdownBuffers();

	// Release the height map data.
	ShutdownHeightMap();

	return;
}


void TerrainClass::Render(ID3D11DeviceContext* deviceContext)
{
	// Put the vertex and index buffers on the graphics pipeline to prepare them for drawing.
	RenderBuffers(deviceContext);

	return;
}


int TerrainClass::GetIndexCount()
{
	return m_indexCount;
}

bool TerrainClass::GenerateHeightMap(ID3D11Device* device, bool keydown, float amplitude)
{
	bool result;
	//the toggle is just a bool that I use to make sure this is only called ONCE when you press a key
	//until you release the key and start again. We dont want to be generating the terrain 500
	//times per second. 
	if(keydown &&(!m_terrainGeneratedToggle))
	{
		//wipe previous height
		for (int i = 0; i < m_terrainHeight; i++)
		{
			for (int z = 0; z < m_terrainWidth; z++)
				m_heightMap[(m_terrainHeight * i) + z].y = 0;

		}

		//random generator seed
		srand(time(NULL));

		//reset perlin noise
		perlinFuncs.Init();

		int index;
		float height = 0.0;

		//loop through the terrain and set the hieghts how we want. This is where we generate the terrain
		//in this case I will run a sin-wave through the terrain in one axis.

 		for(int j=0; j<m_terrainHeight; j++)
		{
			for(int i=0; i<m_terrainWidth; i++)
			{			
				index = (m_terrainHeight * j) + i;


				//PERLIN NOISE HEIGHTS
				if (terrainType == 0)
				{
					m_heightMap[index].y = perlinFuncs.GenerateNoise(m_heightMap[index].x / 10, m_heightMap[index].y / 10, m_heightMap[index].z / 10) * amplitude;
					//debugF = m_heightMap[index].y;
				}

				//cellular automata
				else if(terrainType == 1)
				{
					//COMEPLETELY RANDOM HEIGHTS
					m_heightMap[index].y = RandomHeightField(1300) - 7;
				}
			}
		}

		//runs diamond square once
		if (terrainType == 2)
			DiamondSquare();

		//runs cellular 3 times to allow some smoothing
		else if (terrainType == 1)
			for (int i = 0; i < 10; i++)
				CellularAutomata(0.2);


		result = CalculateNormals();
		if(!result)
		{
			return false;
		}

		//redo texture coordinates
		CalculateTextureCoordinates();

		// Initialize the vertex and index buffer that hold the geometry for the terrain.
		result = InitializeBuffers(device);
		if(!result)
		{
			return false;
		}

		m_terrainGeneratedToggle = true;
	}
	else
	{
		m_terrainGeneratedToggle = false;
	}

	return true;
}
bool TerrainClass::LoadHeightMap(char* filename)
{
	FILE* filePtr;
	int error;
	unsigned int count;
	BITMAPFILEHEADER bitmapFileHeader;
	BITMAPINFOHEADER bitmapInfoHeader;
	int imageSize, i, j, k, index;
	unsigned char* bitmapImage;
	unsigned char height;


	// Open the height map file in binary.
	error = fopen_s(&filePtr, filename, "rb");
	if(error != 0)
	{
		return false;
	}

	// Read in the file header.
	count = fread(&bitmapFileHeader, sizeof(BITMAPFILEHEADER), 1, filePtr);
	if(count != 1)
	{
		return false;
	}

	// Read in the bitmap info header.
	count = fread(&bitmapInfoHeader, sizeof(BITMAPINFOHEADER), 1, filePtr);
	if(count != 1)
	{
		return false;
	}

	// Save the dimensions of the terrain.
	m_terrainWidth = bitmapInfoHeader.biWidth;
	m_terrainHeight = bitmapInfoHeader.biHeight;

	// Calculate the size of the bitmap image data.
	imageSize = m_terrainWidth * m_terrainHeight * 3;

	// Allocate memory for the bitmap image data.
	bitmapImage = new unsigned char[imageSize];
	if(!bitmapImage)
	{
		return false;
	}

	// Move to the beginning of the bitmap data.
	fseek(filePtr, bitmapFileHeader.bfOffBits, SEEK_SET);

	// Read in the bitmap image data.
	count = fread(bitmapImage, 1, imageSize, filePtr);
	if(count != imageSize)
	{
		return false;
	}

	// Close the file.
	error = fclose(filePtr);
	if(error != 0)
	{
		return false;
	}

	// Create the structure to hold the height map data.
	m_heightMap = new HeightMapType[m_terrainWidth * m_terrainHeight];
	if(!m_heightMap)
	{
		return false;
	}

	// Initialize the position in the image data buffer.
	k=0;

	// Read the image data into the height map.
	for(j=0; j<m_terrainHeight; j++)
	{
		for(i=0; i<m_terrainWidth; i++)
		{
			height = bitmapImage[k];
			
			index = (m_terrainHeight * j) + i;

			m_heightMap[index].x = (float)i;
			m_heightMap[index].y = (float)height;
			m_heightMap[index].z = (float)j;

			k+=3;
		}
	}

	// Release the bitmap image data.
	delete [] bitmapImage;
	bitmapImage = 0;

	return true;
}

// RANDOM HEIGHT FIELD
float TerrainClass::RandomHeightField(int amplitude)
{
	//prevent from randoming 0
	if (amplitude <= 0)
		return 0;

	else
	{
		float randomH = rand() % amplitude;

		//get a point between 0 and 1
		randomH = randomH / 100;

		return randomH;
	}

}

//CELLULAR AUTOMATA
void TerrainClass::CellularAutomata(float amount)
{
	int index = 0;

	//fill the array with the current random values 
	for (int j = 0; j < m_terrainHeight; j++)
	{
		for (int i = 0; i < m_terrainWidth; i++)
		{
			index = (m_terrainHeight * j) + i;

			stepping[i][j] = m_heightMap[index].y;
		}
	}

	//cycle through the heights
	for (int j = 0; j < m_terrainHeight; j++)
	{
		for (int i = 0; i < m_terrainWidth; i++)
		{
			index = (m_terrainHeight * j) + i;

			//take the cells, but only modify simultaneously

			//find the difference this current point is from its surroundings and applying the cell rule
			float diff = CellRule(i, j) - stepping[i][j];

			//move this current point towards the average
			stepping[i][j] += diff * amount;
		}
	}

	//make height = array
	//set the height map to the 2D array
	for (int j = 0; j < m_terrainHeight; j++)
	{
		for (int i = 0; i < m_terrainWidth; i++)
		{
			index = (m_terrainHeight * j) + i;

			m_heightMap[index].y = stepping[i][j];
		}
	}
	
}

float TerrainClass::CellRule(int x, int y)
{
	//plans for rules
	//Surrounded by lots of land, increase height
	float mountHeight = 7;
	//surrounded by some land, flatten
	float landHeight = 2;
	//surrounded by very little land, decrease height
	float waterHeight = 0;
	//scale within steepness as well, greater differences in height only appply a half alive maybe
	//take average height concept used in smoothing function and apply so that a large hill will influence slope beneath

	//amount of useable tiles
	int tiles = 0;
	//amount of mountain tiles
	int mountains = 0;
	//amount of hill tiles
	//amount of flat tiles
	int lands = 0;
	//amount of water tiles
	int waters = 0;

	//average of heights for smoothing
	float avgBaseHeight = 0;
	float avgMountHeight = 0;
	float avgLandHeight = 0;
	float avgWaterHeight = 0;

	//2 means will check 24 values (5 x 5 - 1)
	int spread = 2;
	int current = (m_terrainHeight * y) + x;

	//loop through the y coords
	for (int i = y - spread; i <= y + spread; i++)
	{
		//make sure not checking out of the array
		if (i >= 0 && i < m_terrainHeight)
		{
			//do the same for the x coord
			for (int z = x - spread; z <= x + spread; z++)
			{
				//make sure not checking out of the array, or including its own value
				if (z >= 0 && z < m_terrainWidth && ((m_terrainHeight * i) + z) != current)
				{
					int index = (m_terrainHeight * i) + z;

					//check this coordinates height
					if (m_heightMap[index].y > waterHeight)
					{
						//mountainous area
						if (m_heightMap[index].y >= mountHeight)
						{
							mountains++;

							//add the difference in height from regular mountain height to the average
							avgMountHeight += (m_heightMap[index].y - mountHeight);
						}

						//regular land area
						else
						{
							lands++;

							//add the difference in from regular land height to the average
							avgLandHeight += (m_heightMap[index].y - landHeight);
						}					
					}

					//water
					else
					{
						waters++;

						//add the difference in from regular water height to the average
						avgWaterHeight += (m_heightMap[index].y - waterHeight);
					}
					
					tiles++;
					// add the height to a base average
					avgBaseHeight += m_heightMap[index].y;
				}
			}
		}
	}

	//create mean average of all the heights
	avgMountHeight = avgMountHeight / mountains;
	avgLandHeight = avgLandHeight / lands;
	avgWaterHeight = avgWaterHeight / waters;

	//add this onto the base height
	//CHANGE THIS TO += FOR MORE STEEP CLIFF ISLANDS AND = FOR MORE SMOOTH REALISTIC TERRAIN
	avgWaterHeight = waterHeight;
	//add the land height difference
	avgLandHeight += landHeight;
	//add the mountain height difference
	avgMountHeight += mountHeight;

	//avgMountHeight = mountHeight;
	//avgLandHeight = landHeight;
	//avgWaterHeight = waterHeight;

	//create mean average of all heights surrounding point
	avgBaseHeight = avgBaseHeight / tiles;

	//if CURRENT this is a mountain
	if (m_heightMap[current].y >= mountHeight)
	{
		//if nearby area is mountainous, return mountain avg
		if (mountains >= 3)
			return avgMountHeight + avgBaseHeight;			

		//if nearby area has got enough land, return land
		else if (lands >= 9)
			return avgLandHeight + avgBaseHeight;

		//else must be water
		else
			return avgWaterHeight + avgBaseHeight;
	}

	//if CURRENT this is a land
	else if (m_heightMap[current].y >= landHeight)
	{
		//if nearby area is mountainous, return mountain
		if (mountains >= 5)
			return avgMountHeight + avgBaseHeight;

		//if nearby area has got enough land
		else if (lands >= 10)
			return avgLandHeight + avgBaseHeight;

		//else must be water
		else
			return avgWaterHeight + avgBaseHeight;
	}

	//if CURRENT this is a water
	else
	{
		//if nearby area is VERY mountainous, return mountain
		if (mountains >= 8)
			return avgMountHeight + avgBaseHeight;

		//if nearby area has got a lot of land, return land
		else if (lands >= 10)
			return avgLandHeight + avgBaseHeight;

		//else must be water
		else
			return avgWaterHeight + avgBaseHeight;
	}

}

//Diamond Square algorithm fractal
float TerrainClass::DiamondSquare()
{
	//index = (m_terrainHeight * j) + i;

	//wipe the array
	for (int i = 0; i < 129; i++)
	{
		for (int z = 0; z < 129; z++)
			stepping[i][z] = 0;
	}

	//seed all four corners with random
	stepping[0][0] = RandomHeightField(5000) - 15; //5000 -  15
	stepping[0][(m_terrainWidth) - 1] = RandomHeightField(5000) - 15;
	stepping[m_terrainHeight - 1][0] = RandomHeightField(5000) - 15;
	stepping[m_terrainHeight - 1][m_terrainWidth - 1] = RandomHeightField(5000) - 15;

	//set step size (width -1)
	int stepSize = m_terrainWidth - 1;

	//seed the first diamond and square points too
	//midpoint
	stepping[stepSize / 2][stepSize / 2] = RandomHeightField(4000) - 15; //5000 -  15

	stepping[stepSize / 2][0] = RandomHeightField(4000) - 15;
	stepping[stepSize / 2][0] = RandomHeightField(4000) - 15;
	stepping[stepSize / 2][stepSize] = RandomHeightField(4000) - 15;
	stepping[stepSize][stepSize / 2] = RandomHeightField(4000) - 15;

	//half the step size
	stepSize /= 2;

	// random displacement number in range
	int randomRange = 400; //480

	//while the step size is > 1
	while (stepSize > 1)
	{
		//loop through array, and diamond step each square in it
		for (int j = 0; j < m_terrainHeight - 1; j += stepSize)
		{
			for (int i = 0; i < m_terrainWidth - 1; i += stepSize)
			{
				DiamondStep(i, j, stepSize, randomRange);
			}
		}

		//create halfway point of the step, which is where the new diamond corners are
		float halfStep = stepSize / 2;

		//create an even and odd bool
		bool even = true;

		//loop through array, and square step for each diamond in it
		for (int j = 0; j < m_terrainHeight; j += halfStep)
		{
			//create a start point for the square step x coord. If the current y coord is even, use the half step indent
			int startX = 0;
			if (even)
				startX = halfStep;

			for (int i = startX; i < m_terrainWidth; i += stepSize)
			{
				SquareStep(i, j, halfStep, randomRange);
			}

			//flip even bool
			even = !even;
		}

		//divide step size by 2
		stepSize /= 2;
		//reduce random range displacement
		randomRange -= 80;  // -80
		//prevent from going lower than 0
		if (randomRange < 0)
			randomRange = 0;
	}

	//set the height map to the 2D array
	for (int j = 0; j < m_terrainHeight; j++)
	{
		int index;

		for (int i = 0; i < m_terrainWidth; i++)
		{
			index = (m_terrainHeight * j) + i;

			m_heightMap[index].y = stepping[i][j];
		}
	}

	return 0;
}


//DIAMOND STEP
void TerrainClass::DiamondStep(int x, int y, int stepSize, int r)
{
	//pass in bottom left of square (x and y)
	//determine the position of all the corners
	float posBotL = stepping[x][y];
	float posBotR = stepping[x + stepSize][y];
	float posTopL = stepping[x][y + stepSize];
	float posTopR = stepping[x + stepSize][y + stepSize];

	//average of square corners step size apart
	float average = (posBotL + posBotR + posTopL + posTopR) / 4;
	//A[x + step_size/2][y + step_size/2] = avg + r
	stepping[x + (stepSize / 2)][y + (stepSize / 2)] = average + (RandomHeightField(r) - r/100 );
}

//SQUARE STEP
void TerrainClass::SquareStep(int x, int y, int stepSize, int r)
{
	//pass in middle of diamond (x and y)
	// determine the position of the four diamond corners
	//prevent out of bounds
	float posL = 0;
	float posBelow = 0;
	float posR = 0;
	float posAbove = 0;

	//average total
	int avgTotal = 0;

	if (!(x - stepSize < 0))
	{
		posL = stepping[x - stepSize][y];
		avgTotal++;
	}

	if (!(x + stepSize > m_terrainWidth - 1))
	{
		posR = stepping[x + stepSize][y];
		avgTotal++;
	}

	if (!(y + stepSize > m_terrainHeight - 1))
	{
		posAbove = stepping[x][y + stepSize];
		avgTotal++;
	}

	if (!(y - stepSize < 0))
	{
		posBelow = stepping[x][y - stepSize];
		avgTotal++;
	}

	//average of four corners of diamond 
	float average = (posL + posR + posAbove + posBelow) / avgTotal;
	//A[x][y] = avg + r
	stepping[x][y] = average + (RandomHeightField(r) - r / 100);
}


//Smoothing
float TerrainClass::Smoothing(ID3D11Device* device, float amount, float spread, bool keyDown)
{
	bool result;
	int index;
	float smoothedPos = 0;

	if (keyDown && (!m_smoothedToggle))
	{
		for (int j = 0; j < m_terrainHeight; j++)
		{
			for (int i = 0; i < m_terrainWidth; i++)
			{
				//current vertice
				index = (m_terrainHeight * j) + i;

				//set smoothed position equal to current vertex
				smoothedPos = m_heightMap[index].y;

				//create an average height of the nearby vertices
				int valuesUsed = 0;
				//loop through the nearest j coords
				//start with the vertex SPREAD distance back from index and finish with the vertex SPREAD distance along
				for (int k = j - spread; k <= j + spread; k++)
				{
					//make sure not checking out of the array
					if (k >= 0 && k < m_terrainHeight)
					{
						//do the same for the i coord
						for (int l = i - spread; l <= i + spread; l++)
						{
							//make sure not checking out of the array, or including its own height in the average (Although not necessary, this takes peaks down quicker)
							if (l >= 0 && l < m_terrainWidth && ((m_terrainHeight * k) + l) != index)
							{
								//find the height distance this point is from the current
								float dist = m_heightMap[(m_terrainHeight * k) + l].y - m_heightMap[index].y;

								//add this distance to the average
								smoothedPos += dist;

								//increment the number of values used for the average
								valuesUsed++;
							}
						}				
					}
				}
				//divide the position by the number of values used (mean)
				smoothedPos = smoothedPos / valuesUsed;

				//find the distance the average is from the current height
				smoothedPos = smoothedPos - m_heightMap[index].y;

				//move the height to the averaged position, based on the amount passed in
				m_heightMap[index].y += smoothedPos * amount;
			}
		}

		result = CalculateNormals();
		if (!result)
		{
			return false;
		}

		//redo texture coordinates
		CalculateTextureCoordinates();

		// Initialize the vertex and index buffer that hold the geometry for the terrain.
		result = InitializeBuffers(device);
		if (!result)
		{
			return false;
		}

		m_smoothedToggle = true;
	}
	else
	{
		m_smoothedToggle = false;
	}

	return smoothedPos;
}

void TerrainClass::NormalizeHeightMap()
{
	int i, j;


	for(j=0; j<m_terrainHeight; j++)
	{
		for(i=0; i<m_terrainWidth; i++)
		{
			m_heightMap[(m_terrainHeight * j) + i].y /= 15.0f;
		}
	}

	return;
}


bool TerrainClass::CalculateNormals()
{
	int i, j, index1, index2, index3, index, count;
	float vertex1[3], vertex2[3], vertex3[3], vector1[3], vector2[3], sum[3], length;
	VectorType* normals;


	// Create a temporary array to hold the un-normalized normal vectors.
	normals = new VectorType[(m_terrainHeight-1) * (m_terrainWidth-1)];
	if(!normals)
	{
		return false;
	}

	// Go through all the faces in the mesh and calculate their normals.
	for(j=0; j<(m_terrainHeight-1); j++)
	{
		for(i=0; i<(m_terrainWidth-1); i++)
		{
			index1 = (j * m_terrainHeight) + i;
			index2 = (j * m_terrainHeight) + (i+1);
			index3 = ((j+1) * m_terrainHeight) + i;

			// Get three vertices from the face.
			vertex1[0] = m_heightMap[index1].x;
			vertex1[1] = m_heightMap[index1].y;
			vertex1[2] = m_heightMap[index1].z;
		
			vertex2[0] = m_heightMap[index2].x;
			vertex2[1] = m_heightMap[index2].y;
			vertex2[2] = m_heightMap[index2].z;
		
			vertex3[0] = m_heightMap[index3].x;
			vertex3[1] = m_heightMap[index3].y;
			vertex3[2] = m_heightMap[index3].z;

			// Calculate the two vectors for this face.
			vector1[0] = vertex1[0] - vertex3[0];
			vector1[1] = vertex1[1] - vertex3[1];
			vector1[2] = vertex1[2] - vertex3[2];
			vector2[0] = vertex3[0] - vertex2[0];
			vector2[1] = vertex3[1] - vertex2[1];
			vector2[2] = vertex3[2] - vertex2[2];

			index = (j * (m_terrainHeight-1)) + i;

			// Calculate the cross product of those two vectors to get the un-normalized value for this face normal.
			normals[index].x = (vector1[1] * vector2[2]) - (vector1[2] * vector2[1]);
			normals[index].y = (vector1[2] * vector2[0]) - (vector1[0] * vector2[2]);
			normals[index].z = (vector1[0] * vector2[1]) - (vector1[1] * vector2[0]);
		}
	}

	// Now go through all the vertices and take an average of each face normal 	
	// that the vertex touches to get the averaged normal for that vertex.
	for(j=0; j<m_terrainHeight; j++)
	{
		for(i=0; i<m_terrainWidth; i++)
		{
			// Initialize the sum.
			sum[0] = 0.0f;
			sum[1] = 0.0f;
			sum[2] = 0.0f;

			// Initialize the count.
			count = 0;

			// Bottom left face.
			if(((i-1) >= 0) && ((j-1) >= 0))
			{
				index = ((j-1) * (m_terrainHeight-1)) + (i-1);

				sum[0] += normals[index].x;
				sum[1] += normals[index].y;
				sum[2] += normals[index].z;
				count++;
			}

			// Bottom right face.
			if((i < (m_terrainWidth-1)) && ((j-1) >= 0))
			{
				index = ((j-1) * (m_terrainHeight-1)) + i;

				sum[0] += normals[index].x;
				sum[1] += normals[index].y;
				sum[2] += normals[index].z;
				count++;
			}

			// Upper left face.
			if(((i-1) >= 0) && (j < (m_terrainHeight-1)))
			{
				index = (j * (m_terrainHeight-1)) + (i-1);

				sum[0] += normals[index].x;
				sum[1] += normals[index].y;
				sum[2] += normals[index].z;
				count++;
			}

			// Upper right face.
			if((i < (m_terrainWidth-1)) && (j < (m_terrainHeight-1)))
			{
				index = (j * (m_terrainHeight-1)) + i;

				sum[0] += normals[index].x;
				sum[1] += normals[index].y;
				sum[2] += normals[index].z;
				count++;
			}
			
			// Take the average of the faces touching this vertex.
			sum[0] = (sum[0] / (float)count);
			sum[1] = (sum[1] / (float)count);
			sum[2] = (sum[2] / (float)count);

			// Calculate the length of this normal.
			length = sqrt((sum[0] * sum[0]) + (sum[1] * sum[1]) + (sum[2] * sum[2]));
			
			// Get an index to the vertex location in the height map array.
			index = (j * m_terrainHeight) + i;

			// Normalize the final shared normal for this vertex and store it in the height map array.
			m_heightMap[index].nx = (sum[0] / length);
			m_heightMap[index].ny = (sum[1] / length);
			m_heightMap[index].nz = (sum[2] / length);
		}
	}

	// Release the temporary normals.
	delete [] normals;
	normals = 0;

	return true;
}

void TerrainClass::CalculateTextureCoordinates()
{
	//THIS BASE CALCULATING TEXTURE COORDINATES WAS DEMONSTRATED ON RASTERTEK, SEE REPORT FOR DETAILS

	int incrementCount, i, j, tuCount, tvCount;
	float incrementValue, tuCoordinate, tvCoordinate;

	//determine the texture repeat based on the type of plane size
	if (terrainType == 2)
		TEXTURE_REPEAT = 43;
	else
		TEXTURE_REPEAT = 32;


	// Calculate how much to increment the texture coordinates by.
	incrementValue = (float)TEXTURE_REPEAT / (float)m_terrainWidth;

	// Calculate how many times to repeat the texture.
	incrementCount = m_terrainWidth / TEXTURE_REPEAT;

	// Initialize the tu and tv coordinate values.
	tuCoordinate = 0.0f;
	tvCoordinate = 1.0f;

	// Initialize the tu and tv coordinate indexes.
	tuCount = 0;
	tvCount = 0;

	// Loop through the entire height map and calculate the tu and tv texture coordinates for each vertex.
	for (j = 0; j<m_terrainHeight; j++)
	{
		for (i = 0; i<m_terrainWidth; i++)
		{
			// Store the texture coordinate in the height map.
			m_heightMap[(m_terrainHeight * j) + i].tu = tuCoordinate;
			m_heightMap[(m_terrainHeight * j) + i].tv = tvCoordinate;

			// Increment the tu texture coordinate by the increment value and increment the index by one.
			tuCoordinate += incrementValue;
			tuCount++;

			// Check if at the far right end of the texture and if so then start at the beginning again.
			if (tuCount == incrementCount)
			{
				tuCoordinate = 0.0f;
				tuCount = 0;
			}
		}

		// Increment the tv texture coordinate by the increment value and increment the index by one.
		tvCoordinate -= incrementValue;
		tvCount++;

		// Check if at the top of the texture and if so then start at the bottom again.
		if (tvCount == incrementCount)
		{
			tvCoordinate = 1.0f;
			tvCount = 0;
		}
	}

	return;
}



void TerrainClass::ShutdownHeightMap()
{
	if(m_heightMap)
	{
		delete [] m_heightMap;
		m_heightMap = 0;
	}

	return;
}


bool TerrainClass::InitializeBuffers(ID3D11Device* device)
{
	VertexType* vertices;
	unsigned long* indices;
	int index, i, j;
	D3D11_BUFFER_DESC vertexBufferDesc, indexBufferDesc;
    D3D11_SUBRESOURCE_DATA vertexData, indexData;
	HRESULT result;
	int index1, index2, index3, index4;
	//texture coords
	float tu, tv;


	// Calculate the number of vertices in the terrain mesh.
	m_vertexCount = (m_terrainWidth - 1) * (m_terrainHeight - 1) * 6;

	// Set the index count to the same as the vertex count.
	m_indexCount = m_vertexCount;

	// Create the vertex array.
	vertices = new VertexType[m_vertexCount];
	if(!vertices)
	{
		return false;
	}

	// Create the index array.
	indices = new unsigned long[m_indexCount];
	if(!indices)
	{
		return false;
	}

	// Initialize the index to the vertex buffer.
	index = 0;

	// Load the vertex and index array with the terrain data.
	for(j=0; j<(m_terrainHeight-1); j++)
	{
		for(i=0; i<(m_terrainWidth-1); i++)
		{
			index1 = (m_terrainHeight * j) + i;          // Bottom left.
			index2 = (m_terrainHeight * j) + (i+1);      // Bottom right.
			index3 = (m_terrainHeight * (j+1)) + i;      // Upper left.
			index4 = (m_terrainHeight * (j+1)) + (i+1);  // Upper right.

			// Upper left.
			//Tex coords
			tv = m_heightMap[index3].tv;
			//Cover the exact edge
			if (tv == 1.0f) { tv = 0.0f; }

			vertices[index].position = D3DXVECTOR3(m_heightMap[index3].x, m_heightMap[index3].y, m_heightMap[index3].z);
			vertices[index].texture = D3DXVECTOR2(m_heightMap[index3].tu, tv);
			vertices[index].normal = D3DXVECTOR3(m_heightMap[index3].nx, m_heightMap[index3].ny, m_heightMap[index3].nz);
			indices[index] = index;
			index++;

			// Upper right.
			tu = m_heightMap[index4].tu;
			tv = m_heightMap[index4].tv;
			//Cover the exact edge
			if (tu == 0.0f) { tu = 1.0f; }
			if (tv == 1.0f) { tv = 0.0f; }

			vertices[index].position = D3DXVECTOR3(m_heightMap[index4].x, m_heightMap[index4].y, m_heightMap[index4].z);
			vertices[index].texture = D3DXVECTOR2(tu, tv);
			vertices[index].normal = D3DXVECTOR3(m_heightMap[index4].nx, m_heightMap[index4].ny, m_heightMap[index4].nz);
			indices[index] = index;
			index++;

			// Bottom left.
			vertices[index].position = D3DXVECTOR3(m_heightMap[index1].x, m_heightMap[index1].y, m_heightMap[index1].z);
			vertices[index].texture = D3DXVECTOR2(m_heightMap[index1].tu, m_heightMap[index1].tv);
			vertices[index].normal = D3DXVECTOR3(m_heightMap[index1].nx, m_heightMap[index1].ny, m_heightMap[index1].nz);
			indices[index] = index;
			index++;

			// Bottom left.
			vertices[index].position = D3DXVECTOR3(m_heightMap[index1].x, m_heightMap[index1].y, m_heightMap[index1].z);
			vertices[index].texture = D3DXVECTOR2(m_heightMap[index1].tu, m_heightMap[index1].tv);
			vertices[index].normal = D3DXVECTOR3(m_heightMap[index1].nx, m_heightMap[index1].ny, m_heightMap[index1].nz);
			indices[index] = index;
			index++;

			// Upper right.
			//tex coords
			tu = m_heightMap[index4].tu;
			tv = m_heightMap[index4].tv;
			//Cover the exact edge
			if (tu == 0.0f) { tu = 1.0f; }
			if (tv == 1.0f) { tv = 0.0f; }

			vertices[index].position = D3DXVECTOR3(m_heightMap[index4].x, m_heightMap[index4].y, m_heightMap[index4].z);
			vertices[index].texture = D3DXVECTOR2(tu, tv);
			vertices[index].normal = D3DXVECTOR3(m_heightMap[index4].nx, m_heightMap[index4].ny, m_heightMap[index4].nz);
			indices[index] = index;
			index++;

			// Bottom right.
			//tex coords
			tu = m_heightMap[index2].tu;
			//Cover exact edge
			if (tu == 0.0f) { tu = 1.0f; }

			vertices[index].position = D3DXVECTOR3(m_heightMap[index2].x, m_heightMap[index2].y, m_heightMap[index2].z);
			vertices[index].texture = D3DXVECTOR2(tu, m_heightMap[index2].tv);
			vertices[index].normal = D3DXVECTOR3(m_heightMap[index2].nx, m_heightMap[index2].ny, m_heightMap[index2].nz);
			indices[index] = index;
			index++;
		}
	}

	// Set up the description of the static vertex buffer.
    vertexBufferDesc.Usage = D3D11_USAGE_DEFAULT;
    vertexBufferDesc.ByteWidth = sizeof(VertexType) * m_vertexCount;
    vertexBufferDesc.BindFlags = D3D11_BIND_VERTEX_BUFFER;
    vertexBufferDesc.CPUAccessFlags = 0;
    vertexBufferDesc.MiscFlags = 0;
	vertexBufferDesc.StructureByteStride = 0;

	// Give the subresource structure a pointer to the vertex data.
    vertexData.pSysMem = vertices;
	vertexData.SysMemPitch = 0;
	vertexData.SysMemSlicePitch = 0;

	// Now create the vertex buffer.
    result = device->CreateBuffer(&vertexBufferDesc, &vertexData, &m_vertexBuffer);
	if(FAILED(result))
	{
		return false;
	}

	// Set up the description of the static index buffer.
    indexBufferDesc.Usage = D3D11_USAGE_DEFAULT;
    indexBufferDesc.ByteWidth = sizeof(unsigned long) * m_indexCount;
    indexBufferDesc.BindFlags = D3D11_BIND_INDEX_BUFFER;
    indexBufferDesc.CPUAccessFlags = 0;
    indexBufferDesc.MiscFlags = 0;
	indexBufferDesc.StructureByteStride = 0;

	// Give the subresource structure a pointer to the index data.
    indexData.pSysMem = indices;
	indexData.SysMemPitch = 0;
	indexData.SysMemSlicePitch = 0;

	// Create the index buffer.
	result = device->CreateBuffer(&indexBufferDesc, &indexData, &m_indexBuffer);
	if(FAILED(result))
	{
		return false;
	}

	// Release the arrays now that the buffers have been created and loaded.
	delete [] vertices;
	vertices = 0;

	delete [] indices;
	indices = 0;

	return true;
}


void TerrainClass::ShutdownBuffers()
{
	// Release the index buffer.
	if(m_indexBuffer)
	{
		m_indexBuffer->Release();
		m_indexBuffer = 0;
	}

	// Release the vertex buffer.
	if(m_vertexBuffer)
	{
		m_vertexBuffer->Release();
		m_vertexBuffer = 0;
	}

	return;
}


void TerrainClass::RenderBuffers(ID3D11DeviceContext* deviceContext)
{
	unsigned int stride;
	unsigned int offset;


	// Set vertex buffer stride and offset.
	stride = sizeof(VertexType); 
	offset = 0;
    
	// Set the vertex buffer to active in the input assembler so it can be rendered.
	deviceContext->IASetVertexBuffers(0, 1, &m_vertexBuffer, &stride, &offset);

    // Set the index buffer to active in the input assembler so it can be rendered.
	deviceContext->IASetIndexBuffer(m_indexBuffer, DXGI_FORMAT_R32_UINT, 0);

    // Set the type of primitive that should be rendered from this vertex buffer, in this case triangles.
	deviceContext->IASetPrimitiveTopology(D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST);

	return;
}