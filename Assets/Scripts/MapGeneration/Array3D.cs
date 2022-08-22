using UnityEngine;

public class Array3D<T> {
    public Vector3Int size { get; private set; }
    private T[] array;

    public Array3D(Vector3Int size) {
        this.size = size;
        array = new T[GetSize()];
    }

    public int Length { get { return array.Length; } }

    public int GetSize() { return size.x * size.y * size.z; }
    public Vector3Int GetPos(int i) {
        Vector3Int vec = new Vector3Int(0, 0, 0);
        vec.x = i % size.x;
        vec.y = i / size.x % size.y;
        vec.z = i / (size.x * size.y);
        return vec;
    }
    public int GetIndex(Vector3Int vec) {
        return vec.x + vec.y * size.x + vec.z * size.x * size.y;
    }
    public bool InBounds(Vector3Int vec) {
        return vec.x >= 0 && vec.x < size.x &&
                vec.y >= 0 && vec.y < size.y &&
                vec.z >= 0 && vec.z < size.z;
    }

    public T this[int i] {
        get { return array[i]; }
        set { array[i] = value; }
    }
    public T this[Vector3Int vec] {
        get { return array[GetIndex(vec)]; }
        set { array[GetIndex(vec)] = value; }
    }
}