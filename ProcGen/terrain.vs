////////////////////////////////////////////////////////////////////////////////
// Filename: terrain.vs
////////////////////////////////////////////////////////////////////////////////
//cbuffer MatrixBuffer : register(cb0);
//cbuffer TimeBuffer : register(cb1);

/////////////
// GLOBALS //
/////////////
cbuffer MatrixBuffer
{
	matrix worldMatrix;
	matrix viewMatrix;
	matrix projectionMatrix;
	float4 time;
};

//////////////
// TYPEDEFS //
//////////////
struct VertexInputType
{
    float4 position : POSITION;
	float2 tex : TEXCOORD0;
	float3 normal : NORMAL;
};

struct PixelInputType
{
    float4 position : SV_POSITION;
	float2 tex : TEXCOORD0;
	float3 normal : NORMAL;
	float4 globalpos : POSITION;
};


////////////////////////////////////////////////////////////////////////////////
// Vertex Shader
////////////////////////////////////////////////////////////////////////////////
PixelInputType TerrainVertexShader(VertexInputType input)
{
	float timer = 1.0f;

    PixelInputType output;

	float height = 0.1f;
	// Change the position vector to be 4 units for proper matrix calculations.
	input.position.w = 1.0f;

	//make water move based on time and sine wave
	if(time.x > 0)
	{
		//compund sine wave
		input.position.y += height * (sin(input.position.x + time.x * 2) + sin(0.4 * (input.position.x + time.x * 2)) + sin(0.12 *(input.position.x + time.x * 2) + 0.24));

		//modify normals
		input.normal.x = 1 - cos(input.position.x + time.x);
		input.normal.y = abs(cos(input.position.x + time.x));
		//input.normal.y = height * (cos(input.position.x + time.x * 2) + 0.4 * cos(0.4 *(input.position.x + time.x * 2)) + 0.12 * cos(0.12 * (input.position.x + time.x * 2) + 0.24));
	}

	// Calculate the position of the vertex against the world, view, and projection matrices.
    output.position = mul(input.position, worldMatrix);
    output.position = mul(output.position, viewMatrix);
    output.position = mul(output.position, projectionMatrix);
    
	// Store the texture coordinates for the pixel shader.
    output.tex = input.tex;

	// Pass the pixel shader the vetice's global position
	output.globalpos = input.position;

	// Calculate the normal vector against the world matrix only.
    output.normal = mul(input.normal, (float3x3)worldMatrix);
	
    // Normalize the normal vector.
    output.normal = normalize(output.normal);

    return output;
}