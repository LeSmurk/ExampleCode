#include "MarkerHandler.h"


MarkerHandler::MarkerHandler()
{
}


MarkerHandler::~MarkerHandler()
{
}

void MarkerHandler::InitMarker(int id)
{
	//for sample marker number
	markerID = id;

	//null relative positions (6 markers in total)
	for (int i = 0; i < 6; i++)
	{
		relativeMarkerPositions.push_back(gef::Vector4(-1, -1, -1, -1));

		gotRelativePos.push_back(false);

		gef::Matrix44 temp;
		relMat.push_back(temp);
	}

}

void MarkerHandler::SetRelativeMarkerPosition(int otherID, gef::Vector4 otherPos)
{
	//store diretion vector to get to the other marker
	relativeMarkerPositions.at(otherID) = markerMatrix.GetTranslation() - otherPos;
	gef::Matrix44 otherMat;
	

	//got a new one stored
	gotRelativePos.at(otherID) = true;
}

bool MarkerHandler::CheckRelativeMarkerPosition(int relativeID)
{
	//check if has a relative position stored
	if (gotRelativePos.at(relativeID))
		return true;

	return false;
}

gef::Vector4 MarkerHandler::GetRelativeMarkerPosition(int relativeID)
{
	return relativeMarkerPositions.at(relativeID);
}

gef::Matrix44 MarkerHandler::GetRelative(int id)
{
	//returns the local to a given marker
	return relMat.at(id);
}

void MarkerHandler::SetRelative(int id, gef::Matrix44 otherMat)
{
	//set relative

	//get inverse of other matrix
	gef::Matrix44 invMat;
	//otherMat.CalculateDeterminant();
	//invMat.Inverse(otherMat);
	invMat.AffineInverse(otherMat);

	//calculate local in relation ot other matrix
	relMat.at(id) = (markerMatrix * invMat);

	//got a relative to this marker
	gotRelativePos.at(id) = true;
}
