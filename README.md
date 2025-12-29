# Kernel Automata
Exploring artificial life emerging from <a href="https://medium.com/@davidstanojevic43/smoothlife-a-simple-mathematical-overview-478876a6d1ab" target="_blank">continous cellular automata</a>, extension of <a href="https://en.wikipedia.org/wiki/Conway%27s_Game_of_Life" target="_blank">Conway's "Game of Life</a>. <br/>

Inspired by:
* https://www.youtube.com/watch?v=6kiBYjvyojQ
* https://www.youtube.com/watch?v=8wDSQxmAyTw
* https://github.com/Chakazul/Lenia
* https://github.com/duckythescientist/SmoothLife

## Features
* Convolutions and growth implemented with GPU compute shaders. Optimized algorithm using FFT (Cooley)
* One or two species
* Configurable kernel rings (up to 5)
* Configurable growth function (one or two bell-curve spikes)
* Everything configurable in UI
* Save/Load configurations from JSON

## GUI

| <p align="center"><img height="1019" height="1019" src="https://github.com/panjanek/kernel-automata/blob/22ac1c684802494763a9403ab90b93c88014ab0f/captures/fullsize.png" /><br/>field</p> | <p align="center"><img src="https://github.com/panjanek/kernel-automata/blob/22ac1c684802494763a9403ab90b93c88014ab0f/captures/screen.png"/><br/>controls</p> |
|---|---|

## Observed patterns

| <p align="center"><img src="https://github.com/panjanek/kernel-automata/blob/e174186b1fdfb0dcfb6b381cf197920b19dc2ddc/captures/orb.gif" /><br/>orbs</p> | <p align="center"><img src="https://github.com/panjanek/kernel-automata/blob/e174186b1fdfb0dcfb6b381cf197920b19dc2ddc/captures/double-orb.gif" /><br/>double orb</p> | <p align="center"><img src="https://github.com/panjanek/kernel-automata/blob/e174186b1fdfb0dcfb6b381cf197920b19dc2ddc/captures/cells.gif" /><br/>mitosis</p> |
|---|---|---|
| <p align="center"><img src="https://github.com/panjanek/kernel-automata/blob/2ea4a051998bc050501404e586366620ee2dd7c3/captures/hoppers2.gif" /><br/>hoppers</p> | <p align="center"><img src="https://github.com/panjanek/kernel-automata/blob/2ea4a051998bc050501404e586366620ee2dd7c3/captures/orb-tail.gif" width="224" height="201" /><br/>snakes</p> | <p align="center"><img src="https://github.com/panjanek/kernel-automata/blob/2ea4a051998bc050501404e586366620ee2dd7c3/captures/swirl-hoppers.gif" /><br/>swirls</p> |
| <p align="center"><img src="https://github.com/panjanek/kernel-automata/blob/043639bd75033ef8e599d1ce348334fbab1c2bf9/captures/tapes.gif" /><br/>tapeworms</p> | <p align="center"><img src="https://github.com/panjanek/kernel-automata/blob/043639bd75033ef8e599d1ce348334fbab1c2bf9/captures/big-cell.gif" /><br/>cell</p> | <p align="center"><img src="https://github.com/panjanek/kernel-automata/blob/043639bd75033ef8e599d1ce348334fbab1c2bf9/captures/throbe.gif" /><br/>throbes</p> |
| <p align="center"><img src="https://github.com/panjanek/kernel-automata/blob/06f29e250804bb87aff9a0cc7f308d7d8ac9a8b9/captures/displace.gif" /><br/>displace</p> | <p align="center"><img src="https://github.com/panjanek/kernel-automata/blob/06f29e250804bb87aff9a0cc7f308d7d8ac9a8b9/captures/lattice.gif" /><br/>growth</p> | <p align="center"><img src="https://github.com/panjanek/kernel-automata/blob/143ca6ec22c50b5e34db2e8c0dfa8cca58487646/captures/net.gif" /><br/>net</p> |
