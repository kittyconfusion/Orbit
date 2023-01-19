import matplotlib.pyplot as plt
import numpy as np

x = []
y = []

for line in open("xy.txt", 'r'):
    line = line.strip()
    tx = line.split(' ')[0]
    ty = line.split(' ')[1]
    x.append((float(tx)))
    y.append((float(ty)))


xpoints = np.array(x)
ypoints = np.array(y)

plt.plot(xpoints, ypoints, 'o')
plt.scatter(0, 0, color="y")
plt.axis('square')
plt.show()