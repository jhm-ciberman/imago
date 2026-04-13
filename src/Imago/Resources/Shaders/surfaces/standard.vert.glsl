// Standard vertex shader - no custom transformation

VertexOutput Vertex(VertexInput vertexInput)
{
    VertexOutput o;
    o.Position = vertexInput.WorldPosition;
    o.Normal = vertexInput.Normal;
    return o;
}
