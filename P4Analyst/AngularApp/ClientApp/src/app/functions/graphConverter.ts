import { Node } from '../models/node';
import { NodeShape } from '../models/nodeShape';
import { Edge } from '../models/edge';
import { EdgeArrowType } from '../models/edgeArrowType';
import { EdgeStyle } from '../models/edgeStyle';

export function getSubGraph(id: string, graph: Array<Node>): string {
    const subGraphNodes = graph.filter(x => x.subGraph === id);

    if (subGraphNodes.length === 0) {
        return '';
    }

    let subGraph = `subgraph "${id}" {`;
    subGraphNodes.forEach(node => {
        subGraph += getNodeText(node);
        node.edges.filter(x => graph.find(y => y.id === x.child).subGraph === node.subGraph).forEach(item => {
            subGraph += getEdgeText(item, graph);
        });
    });
    subGraph += ' } ';

    return subGraph;
}

export function getNodeText(node: Node): string {
    return `"${node.id}" [id="${node.id}" label="${node.text}" tooltip="${node.tooltip}" fillcolor="${node.fillColor}" shape="${getNodeShape(node.shape)}"];`;
}

export function getEdgeText(edge: Edge, graph: Array<Node>): string {
    const parentNode = graph.find(x => x.id === edge.parent);
    const childNode = graph.find(x => x.id === edge.child);

    let inResult = `id="${edge.parent} -> ${edge.child}" color="${edge.color}" arrowType="${getEdgeArrowType(edge.edgeArrowType)}" style="${getEdgeStyle(edge.edgeStyle)}"`;

    if ((parentNode.subGraph !== '' || childNode.subGraph !== '') && parentNode.subGraph !== childNode.subGraph) {
        if (parentNode.subGraph !== '') {
            inResult += ` ltail="${parentNode.subGraph}"`;
        }
        if (childNode.subGraph !== '') {
            inResult += ` lhead="${childNode.subGraph}"`;
        }
    }

    return `"${edge.parent}" -> "${edge.child}" [${inResult}];`;
}

export function getNodeShape(i: number): string {
    switch (i) {
        case 0:
            return NodeShape.Ellipse;
        case 1:
            return NodeShape.Diamond;
        case 2:
            return NodeShape.Box;
        case 3:
            return NodeShape.Circle;
    }
}

export function getEdgeArrowType(i: number): string {
    switch (i) {
        case 0:
            return EdgeArrowType.Normal;
        case 1:
            return EdgeArrowType.None;
    }
}

export function getEdgeStyle(i: number): string {
    switch (i) {
        case 0:
            return EdgeStyle.None;
        case 1:
            return EdgeStyle.Dotted;
    }
}
