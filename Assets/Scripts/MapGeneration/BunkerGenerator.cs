using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace MapGeneration.BunkerGeneration
{
    public class BunkerGenerator : MonoBehaviour
    {

        [SerializeField] private ObjectPool TestObjects;

        private static Vector3Int[] CORRIDOR_DIRECTIONS = {
            new Vector3Int(1, 0, 0), new Vector3Int(-1, 0, 0)
            , new Vector3Int(0, 0, 1), new Vector3Int(0, 0, -1) };

        private int tempInt;
        private Vector3Int tempV3;

        private List<Vector3Int> endPoints;

        [SerializeField] private float corridorExtensionProbability;
        [SerializeField] private int corridorLength;
        [SerializeField] private int maxNumCorridors;
        private int currentNumCorridors;

        private int[,,] roomDisposition;

        public void GenerateBunker(Vector3 startingPosition)
        {
            Vector3Int arrayIndexStart = new Vector3Int((int)startingPosition.x + 500, (int)startingPosition.y + 500, (int)startingPosition.z);

            Debug.Log("Generating Bunker");

            tempV3 = new();
            endPoints = new();

            roomDisposition = new int[1000, 1000, 3];

            while (endPoints.Count > 0)
            {
                int dirIndex = GetRandomDirection(endPoints[0]);

                if (dirIndex != -1 && currentNumCorridors < maxNumCorridors)
                {
                    AddCoriddorAndAddEndPoint(
                        corridorLength,
                        endPoints[0],
                        CORRIDOR_DIRECTIONS[tempInt]);

                    currentNumCorridors++;
                }
            }
        }

        private void PlaceRooms(Vector3Int startPos)
        {
            List<Vector3Int> rooms = new();
            rooms.Add(startPos);
            
            while(rooms.Count > 0)
            {
                TestObjects.Pull(true).transform.position = rooms[0];

                
            }
        }

        private void AddRoomsToPlace(Vector3Int currentRoomPos,List<Vector3Int> rooms)
        {
            for (int i = 0; i < CORRIDOR_DIRECTIONS.Length; i++)
            {
                tempV3 = new(
                    currentRoomPos.x + CORRIDOR_DIRECTIONS[i].x,
                    currentRoomPos.y + CORRIDOR_DIRECTIONS[i].y,
                    currentRoomPos.z + CORRIDOR_DIRECTIONS[i].z);

                if (roomDisposition[tempV3.x,tempV3.y,tempV3.z] != 0)
                {
                    rooms.Add(new Vector3Int(tempV3.x,tempV3.y,tempV3.z));
                } 
            }
        }

        private Vector3Int AddCoriddorAndAddEndPoint(int length, Vector3Int startPosition, Vector3Int direction)
        {
            currentNumCorridors++;

            for (int i = 0; i < length; i++)
            {
                tempV3.x = startPosition.x + direction.x * i;
                tempV3.y = startPosition.y + direction.y * i;
                tempV3.z = startPosition.z + direction.z * i;

                if (roomDisposition[tempV3.x, tempV3.y, tempV3.z] == 0)
                {
                    roomDisposition[tempV3.x, tempV3.y, tempV3.z] = 1;
                }
            }

            endPoints.Add(tempV3);
            return tempV3;
        }

        private int GetRandomDirection(Vector3Int position)
        {
            tempInt = Random.Range(0, 3);

            if (roomDisposition[
                position.x + CORRIDOR_DIRECTIONS[tempInt].x,
                position.y + CORRIDOR_DIRECTIONS[tempInt].y,
                position.z + CORRIDOR_DIRECTIONS[tempInt].z] == 0)
            {
                return tempInt;
            }

            return -1;
        }

        public void GenerateBunker()
        {
            GenerateBunker(Vector3.zero);
        }
    }
}
