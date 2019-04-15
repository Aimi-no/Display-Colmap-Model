# -*- encoding: utf-8 -*-
# pip install flask

import flask
from flask import request
from PIL import Image
import numpy as np
import time
import queue
import os
import math
import struct


app = flask.Flask(__name__)


folder = ""



# Configure the flask server
app.config['JSON_SORT_KEYS'] = False


def calculate_transform(holCams, colCams):
    print("Calculating transform")
    transformation = {}

    numOfCams = len(holCams)
    mHol = np.array([0, 0, 0])
    mCol = np.array([0, 0, 0])
    for i in range(numOfCams):
        mHol = mHol + holCams[i]
        mCol = mCol + colCams[i]

    mHol = mHol / numOfCams  #a.mean(axis=1)
    mCol = mCol / numOfCams
    Q = np.array([[1, 0, 0], [0, -1, 0], [0, 0, 1]])
    allCol = np.zeros((3, numOfCams))
    allHol = np.zeros((3, numOfCams))


    for i in range(numOfCams):
        holCams[i] = holCams[i] - mHol
        colCams[i] = colCams[i] - mCol
        x = Q * colCams[i]
        colCams[i] = x.diagonal()
        allCol[0, i] = colCams[i][0]
        allCol[1, i] = colCams[i][1]
        allCol[2, i] = colCams[i][2]

        allHol[0, i] = holCams[i][0]
        allHol[1, i] = holCams[i][1]
        allHol[2, i] = holCams[i][2]

    holNorm  = np.linalg.norm(allHol)
    colNorm = np.linalg.norm(allCol)

    scale = holNorm / colNorm
    transformation['scale'] = scale

    for i in range(numOfCams):
        colCams[i] = scale * colCams[i]


    allCol = scale * allCol.transpose()

    allHol = allHol.transpose()

    (n,m) = allHol.shape
    (ny,my) = allCol.shape


    muX = allHol.transpose().mean(axis=1)
    muY = allCol.transpose().mean(axis=1)
    allHol0 = allHol - np.tile(muX, (n, 1))


    allCol0 = allCol - np.tile(muY, (n, 1))


    sqHol = np.zeros(allHol0.shape)
    np.square(allHol0, out = sqHol)

    ssqX = np.sum(sqHol.transpose(), 1)

    sqCol = np.zeros(allCol0.shape)
    np.square(allCol0, out = sqCol)


    ssqY = np.sum(sqCol.transpose(), 1)


    constX = np.all(ssqX <= abs((np.finfo(type(allHol[0, 0])).eps * n * muX)) ** 2)
    constY = np.all(ssqY <= abs((np.finfo(type(allCol[0, 0])).eps * n * muY)) ** 2)

    ssqX = np.sum(ssqX)

    ssqY = np.sum(ssqY)



    if not constX and not constY:
        normX = np.sqrt(ssqX)
        normY = np.sqrt(ssqY)

        allHol0 = allHol0 / normX
        allCol0 = allCol0 / normY


        A = np.matmul(allHol0.transpose(), allCol0)
        U, S, V = np.linalg.svd(A)
        R = np.matmul(V, np.transpose(U))

        print("----SHAPES----")
        print(U.shape)
        print(V.shape)
        print(R.shape)


        traceTA = np.sum(S)
        b = 1
        d = 1 + ssqY / ssqX - 2 * traceTA * normY / normX
        #Z = normY*Y0 * T + repmat(muX, n, 1);
        c = muX - b * np.matmul(muY,R)
        #transform = struct('T', R, 'b', b, 'c', repmat(c, n, 1));
        transformation['rotation1'] = R[0].tolist()
        transformation['rotation2'] = R[1].tolist()
        transformation['rotation3'] = R[2].tolist()
        transformation['translation'] = c.tolist()


        print("----TOLIST----")
        print(c)
        print(R)

    # The degenerate cases: X all the same, and Y all the same.
    elif constX:
        d = 0
        #Z = repmat(muX, n, 1);
        R = np.eye(my, m)
        transformation['rotation1'] = R[0].tolist()
        transformation['rotation2'] = R[1].tolist()
        transformation['rotation3'] = R[2].tolist()
        transformation['translation'] = muX.tolist()

        print("----TOLIST----")
        print(muX)
        print(R)


    #!constX & constY
    else:
        d = 1
        R = np.eye(my, m)
        transformation['rotation1'] = R[0].tolist()
        transformation['rotation2'] = R[1].tolist()
        transformation['rotation3'] = R[2].tolist()
        transformation['translation'] = muX.tolist()

        print("----TOLIST----")
        print(muX)
        print(R)


    return transformation


@app.route("/api/cv/create_folder/", methods=['POST'])
def api_create_folder():
    try:
        global folder
        folder = str(int(time.time()))
        path = os.path.dirname(os.path.realpath(__file__)) + "/"
        if not os.path.exists(folder):
            os.makedirs(path + folder)

            print("created folder: " + folder)


            response_list = [("folder", folder)]

            response_dict = dict(response_list)

            return flask.jsonify(response_dict)

    except Exception as err:
        print("ko:", err)

    return "ok"



@app.route("/api/cv/save_images/", methods=['POST'])
def api_save_images():
    try:
        for f in request.files:
            img = request.files[f]

            global folder

            start = time.time()
            pimg = Image.open(img.stream)
            path = os.path.dirname(os.path.realpath(__file__)) + "/"
            #npimg = np.array(pimg)
            filename = path + folder + "/" + str(start).replace('.', '_') + "_" + str(f) + ".jpg"
            print("saving  as: " + filename)

            pimg.save(filename);

            response_list = [("file", filename), ("folder", folder)]

            response_dict = dict(response_list)

            #print(response_dict)
            end = time.time()
            print("Processing time: {} ".format(end - start))
            return flask.jsonify(response_dict)

    except Exception as err:
        print("ko:", err)

    return "ok"



@app.route("/api/cv/run_reconstruction/", methods=['POST'])
def api_run_reconstruction():
    try:
        global folder
        path = os.path.dirname(os.path.realpath(__file__)) + "/"
        os.system(path + "COLMAP.bat automatic_reconstructor \ --workspace_path " + path + folder + " \ --image_path " + path + folder + " --quality medium")

        response_list = [("folder", folder)]

        response_dict = dict(response_list)


        return flask.jsonify(response_dict)

    except Exception as err:
        print("ko:", err)

    return "ok"


@app.route("/api/cv/get_transformation/", methods=['POST'])
def api_get_transformation():
    try:
        global folder
        path = os.path.dirname(os.path.realpath(__file__)) + "/"
        os.system(path + 'COLMAP.bat model_converter --input_path ' + path + folder + '/dense/0/sparse --output_path ' + path + folder + '/dense/0/sparse --output_type TXT')
        print()
        numOfCams = request.json['numberOfCams']
        #print()
        #print(request.json)

        holCams = {}
        #cams = request.json['cameras']

        for i in range(0,numOfCams):
            holCams[i] = np.array([float(request.json['cameras'][i]['position']['x']), float(request.json['cameras'][i]['position']['y']), float(request.json['cameras'][i]['position']['z'])])
            #print(holCams[i])

        with open(path + folder + "/dense/0/sparse/images.txt", 'r') as file:
            line = file.readline()
            while not line.startswith('# Number of images:'):
                line = file.readline()

            #print(line)
            numOfColCams = int(line.split(' ')[4].split(',')[0])
            if numOfColCams != numOfCams:
                #print('Number of cameras does not match, can\'t calculate transformation\n')
                raise NameError('Number of cameras does not match, can\'t calculate transformation')

            colCams = {}
            for i in range(numOfColCams):
                info = file.readline()
                file.readline()
                parsed = info.split(' ')
                # print(parsed)
                colCams[int(parsed[0]) - 1] = np.array([float(parsed[5]), float(parsed[6]), float(parsed[7])])

        transform = calculate_transform(holCams, colCams)


        response_list = transform



        response_dict = dict(response_list)
        return flask.jsonify(response_dict)

    except Exception as err:
        print("ko:", err)

    return "ok"


@app.route("/api/cv/query_reconstruction/", methods=['GET'])
def api_query_reconstruction():
    try:
        global folder
        path = os.path.dirname(os.path.realpath(__file__)) + "/"
        with open(path + folder + "/dense/0/fused.ply", 'rb') as myfile:
            data = myfile.read()

        return bytes(data)

    except Exception as err:

        print("ko:", err)

@app.route("/api/cv/query_cameras/", methods=['GET'])
def api_query_cameras():
    try:
        global folder
        path = os.path.dirname(os.path.realpath(__file__)) + "/"
        os.system(path + 'COLMAP.bat model_converter --input_path ' + path + folder + '/dense/0/sparse --output_path '+ path + folder + '/dense/0/sparse --output_type TXT')

        with open(path + folder + "/dense/0/sparse/images.txt", 'r') as myfile:
            data = myfile.read()

        return data

    except Exception as err:

        print("ko:", err)


def decimate(verts, colours, faces):
    print("Decimating")
    numOfVerts = len(verts)
    while numOfVerts > 65000:
        #decimuj
        print(str(numOfVerts) + " vertices")
        q = queue.PriorityQueue(maxsize = numOfVerts * numOfVerts)
        for i in range(0, numOfVerts):
            a = verts[i]
            for j in range(i + 1, numOfVerts):
                b = verts[j]
                d = b - a
                d = np.square(d)
                d = np.sum(d)
                dist = np.sqrt(d)
                q.put((dist, i, j))
               # print((dist, i, j))
        
        (dist, a, b) = q.get()
        verts[a] = (verts[a] + verts[b]) / 2

        verts.pop(b)

        numOfVerts = numOfVerts - 1

        matches = 0 # je potreba?
        numOfFaces = len(faces)

        same = np.nonzero(faces == b)
        for i in same:
            faces[i] = a

        greater = np.nonzero(faces > b)
        for i in greater:
            faces[i] = faces[i] - 1


        '''for i in range(0, numOfFaces):
            if b in faces[i]:
                matches += 1
                idx = faces[i].index(b)
               # print(faces[i])
                if a in faces[i]:
                    faces[i].pop(idx)
                    if len(faces[i]) < 3:
                        faces.pop(i)
                        i -= 1
                        numOfFaces -= 1
                else:
                    faces[i][idx] = a
            changedIdx = [i for i,v in enumerate(faces[i]) if v > b]
            for j in range(0,len(changedIdx)):
                faces[i][j] -= 1'''
            
    return verts, colours, faces

@app.route("/api/cv/download_model/", methods=['GET'])
def api_download_model():
    try:
        global folder
        print("Getting path")
        path = os.path.dirname(os.path.realpath(__file__)) + "/"
        filename = path + "1547206402/dense/0/decimated.ply"

        print("Getting file")
        verts = []
        colours = []
        faces = []

        with open(filename, 'r') as myfile:
            while True:
                data = myfile.readline()
                #print(data)
                if data.startswith('element vertex'):
                    numOfVerts = int(data.split(' ')[2].split('\n')[0])
                    print("Number of vertices: " + str(numOfVerts))
                elif data.startswith('element face'):
                    numOfFaces= int(data.split(' ')[2].split('\n')[0])
                    print("Number of faces: " + str(numOfFaces))
                elif data.startswith('end_header'):
                    break
            for i in range(0, numOfVerts):
                data = myfile.readline()
                parsed = data.split('\n')[0]
                parsed = parsed.split(' ')
                verts.append(float(parsed[0]))
                verts.append(float(parsed[1]))
                verts.append(float(parsed[2]))

                colours.append(int(parsed[3]))
                colours.append(int(parsed[4]))
                colours.append(int(parsed[5]))
                #print(parsed)



            for j in range(0, numOfFaces):
                data = myfile.readline()
                parsed = data.split('\n')[0]
                parsed = parsed.split(' ')
                num =  int(parsed[0])
                if not num == 3:
                    print(num)
                    raise ValueError('Face is not a triangle!')

                faces.append(int(parsed[1]))
                faces.append(int(parsed[2]))
                faces.append(int(parsed[3]))
                #print([num,int(parsed[1]), int(parsed[2]), int(parsed[3])])

       # verts, colours, faces = decimate(verts, colours, faces)
        mesh = {}
        print("Done")

        mesh['verts'] = verts
        mesh['faces'] = faces
        mesh['cols'] = colours

        response_list = mesh
        response_dict = dict(response_list)
        return flask.jsonify(response_dict)

    except Exception as err:

        print("ko:", err)


if __name__ == "__main__":
    # app.config.update(MAX_CONTENT_LENGTH=100*10**6)
    app.run(port=9099
,host='192.168.56.1')

	                                   
	                                   
