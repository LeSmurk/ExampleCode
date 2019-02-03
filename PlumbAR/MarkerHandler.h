#pragma once
#include <maths/vector2.h>
#include <maths/vector4.h>
#include <maths/matrix44.h>
#include <vector>


class MarkerHandler
{
public:
	MarkerHandler();
	~MarkerHandler();

	int markerID = -1;

	//no viable markers
	bool noMarkers = true;

	//initialise marker class
	void InitMarker(int);

	gef::Matrix44 markerMatrix;
	gef::Matrix44 lastKnownMatrix;

	//use other markers and the last known position of this marker relative to those to create temporary position
	void SetRelativeMarkerPosition(int, gef::Vector4);

	//chekc if there is a stored marker relative
	bool CheckRelativeMarkerPosition(int);

	//return relative marker position
	gef::Vector4 GetRelativeMarkerPosition(int);

	gef::Matrix44 GetRelative(int);
	void SetRelative(int, gef::Matrix44);

private:

	std::vector<gef::Vector4> relativeMarkerPositions;
	std::vector<bool> gotRelativePos;

	std::vector<gef::Matrix44> relMat;

	//know if the marker is visible
	bool markerVisible = false;

};

