////////////////////////////////////////////////////////////////////////////////
// Filename: terrainclass.h
////////////////////////////////////////////////////////////////////////////////
#ifndef _TERRAINCLASS_H_
#define _TERRAINCLASS_H_


//////////////
// INCLUDES //
//////////////
#include <d3d11.h>
#include <d3dx10math.h>
#include <stdio.h>
#include "perlinclass.h"
#include "textureclass.h"

////////////////////////////////////////////////////////////////////////////////
// Class name: TerrainClass
////////////////////////////////////////////////////////////////////////////////

class TerrainClass
{
private:
	struct VertexType
	{
		D3DXVECTOR3 position;
		D3DXVECTOR2 texture;
	    D3DXVECTOR3 normal;
	};

	struct HeightMapType 
	{ 
		float x, y, z;
		float tu, tv;
		float nx, ny, nz;
	};

	struct VectorType 
	{ 
		float x, y, z;
	};

public:
	TerrainClass();
	TerrainClass(const TerrainClass&);
	~TerrainClass();

	bool Initialize(ID3D11Device*, char*);
	bool InitializeTerrain(ID3D11Device*, int terrainWidth, int terrainHeight, WCHAR*, WCHAR*, WCHAR*, WCHAR*, WCHAR*);
	void Shutdown();
	void Render(ID3D11DeviceContext*);
	bool GenerateHeightMap(ID3D11Device* device, bool keydown, float);
	int  GetIndexCount();

	//textures
	TextureClass *grassTexture, *slopeTexture, *rockTexture, *snowTexture, *sandTexture;

private:
	bool LoadHeightMap(char*);
	void NormalizeHeightMap();
	bool CalculateNormals();
	//USED A RASTERTEK FUNCTION FOR THIS - WILL NEED TO GO IN DOCS
	void CalculateTextureCoordinates();
	void ShutdownHeightMap();

	bool InitializeBuffers(ID3D11Device*);
	void ShutdownBuffers();
	void RenderBuffers(ID3D11DeviceContext*);

	//texturing
	//43 for 129 plane and 32 for 128 plane
	int TEXTURE_REPEAT = 32;

	//height map
	float RandomHeightField(int);
	float DiamondSquare();
	perlinclass perlinFuncs;

	//Cellular
	void CellularAutomata(float);
	float CellRule(int, int);

	//create 2d array
	//Yes, this these are bad floating numbers
	float stepping[129][129] = { 0 };
	//diamond step
	void DiamondStep(int, int, int, int);
	//square step
	void SquareStep(int, int, int, int);

public:
	//smoothing
	float Smoothing(ID3D11Device* device, float amount, float spread, bool keyDown);
	float debugF = -2.43;

	//current terrain type, 0 is basic perlin (128x128), 1 is cellular (128x128) and 2 is dia square (129x129)
	int terrainType = 0;
	
private:
	bool m_terrainGeneratedToggle, m_smoothedToggle, m_growthToggle;
	int m_terrainWidth, m_terrainHeight;
	int m_vertexCount, m_indexCount;
	ID3D11Buffer *m_vertexBuffer, *m_indexBuffer;
	HeightMapType* m_heightMap;
};

#endif