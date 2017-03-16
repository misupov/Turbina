"use strict";

class Rope {
    constructor(from, to) {
        this.setupAnchors(from, to);
    }

    setupAnchors(from, to) {
        this.isStable = false;
        this.from = from;
        this.to = to;
        this.reset();
    }

    reset() {
        this.oldPosition = { from: { x: this.from.x, y: this.from.y }, to: { x: this.to.x, y: this.to.y } };
        this.ropeSimulation = new RopeSimulation(15, 1, 1000, 20, 2, this.from, this.to);
    }

    getPath() {
        var result = [];
        this.ropeSimulation.masses.forEach(m => result.push({ x: m.pos.x, y: m.pos.y }));
        return result;
    }

    simulate(elapsedMilliseconds) {
        if (this.needsSimulation()) {
            this.simulate2(this.from, this.to, Math.min(10, elapsedMilliseconds));
            this.oldPosition = { from: { x: this.from.x, y: this.from.y }, to: { x: this.to.x, y: this.to.y } };
        }
    }

    needsSimulation() {
        return !this.isStable ||
            this.oldPosition.from.x !== this.from.x ||
            this.oldPosition.from.y !== this.from.y ||
            this.oldPosition.to.x !== this.to.x ||
            this.oldPosition.to.y !== this.to.y;
    }

    simulate2(newStart, newEnd, elapsedMilliseconds) {
        let velocity = 0;
        for (let i = 0; i < 8; i++) {
            this.ropeSimulation.from = newStart;
            this.ropeSimulation.to = newEnd;
            this.ropeSimulation.operate(elapsedMilliseconds / 1000);
            this.ropeSimulation.masses.forEach(m => {
                velocity += m.vel.x * m.vel.x + m.vel.y * m.vel.y;
            });
        }

        this.isStable = velocity < 0.1;
    }
}

class RopeSimulation {
    constructor(numOfMasses, mass, springConstant, springFrictionConstant, airFrictionConstant, from, to) {
        this.numOfMasses = numOfMasses;
        this.mass = mass;
        this.springConstant = springConstant;
        this.springFrictionConstant = springFrictionConstant;
        this.airFrictionConstant = airFrictionConstant;
        this.from = from;
        this.to = to;
        this.reset();
    }

    reset() {
        const dx = this.to.x - this.from.x;
        const dy = this.to.y - this.from.y;
        this.springs = [];
        this.masses = [];
        this.masses.push(new Mass(this.mass, this.from.x, this.from.y));
        for (let i = 1; i <= this.numOfMasses; i++) {
            this.masses.push(new Mass(this.mass, this.from.x + dx * (i / this.numOfMasses), this.from.y + dy * (i / this.numOfMasses)));
            this.springs.push(new Spring(this.masses[i - 1], this.masses[i], this.springConstant, this.springFrictionConstant));
        }
    }

    init() {
        for (let i = 0; i < this.masses.length; i++) {
            this.masses[i].init();
        }
    }

    solve() {
        for (let i = 0; i < this.springs.length; i++) {
            this.springs[i].solve();
        }

        let k = 0;
        const width = Math.abs(this.masses[0].pos.x - this.masses[this.masses.length - 1].pos.x);
        const height = Math.abs(this.masses[0].pos.y - this.masses[this.masses.length - 1].pos.y);

        for (let i = 0; i < this.masses.length; i++) {
            //                mass.ApplyForce(new Vector(0, 1000) * mass.M); //The gravitational force

            let mass = this.masses[i];
            mass.applyForce(-mass.vel.x * this.airFrictionConstant, -mass.vel.y * this.airFrictionConstant); //The air friction

            let x = (this.masses.length / 2 - i) * (Math.sqrt(width*width + height*height))*2;// * Math.abs(height) * 0.5;
//            let x = (this.masses.Count / 2 - i) * height * 3;
                
            mass.applyForce(x * mass.mass, 0);

            k++;
        }
    }

    operate(dt) {
        this.init(); // Step 1: reset forces to zero
        this.solve(); // Step 2: apply forces
        this.simulate(dt); // Step 3: iterate the masses by the change in time
    }

    simulate(dt) {
        this.masses[0].pos.x = this.from.x;
        this.masses[0].pos.y = this.from.y;
        this.masses[0].vel.x = 0;
        this.masses[0].vel.y = 0;
        this.masses[this.masses.length - 1].pos.x = this.to.x;
        this.masses[this.masses.length - 1].pos.y = this.to.y;
        this.masses[this.masses.length - 1].vel.x = 0;
        this.masses[this.masses.length - 1].vel.y = 0;

        for (let i = 1; i < this.masses.length - 1; i++)
        {
            this.masses[i].simulate(dt);
        }
    }
}

class Mass {
    constructor(mass, x, y) {
        this.mass = mass;
        this.pos = { x: x, y: y };
        this.vel = { x: 0, y: 0 };
        this.force = { x: 0, y: 0 };
    }

    init() {
        this.force = { x: 0, y: 0 };
    }

    simulate(dt) {
        this.vel.x += (this.force.x / this.mass) * dt;
        this.vel.y += (this.force.y / this.mass) * dt;
        // Change in velocity is added to the velocity.
        // The change is proportinal with the acceleration (force / m) and change in time

        this.pos.x += this.vel.x * dt;
        this.pos.y += this.vel.y * dt;
        // Change in position is added to the position.
        // Change in position is velocity times the change in time
    }

    applyForce(forceX, forceY)
    {
        this.force.x += forceX;
        this.force.y += forceY;
        // The external force is added to the force of the mass
    }
}

class Spring {
    constructor(mass1, mass2, springConstant, frictionConstant) {
        this.mass1 = mass1;
        this.mass2 = mass2;
        this.springConstant = springConstant;
        this.frictionConstant = frictionConstant;
    }

    solve() {
        const springVectorX = this.mass2.pos.x - this.mass1.pos.x;
        const springVectorY = this.mass2.pos.y - this.mass1.pos.y;
        //vector between the two masses

        let forceX = springVectorX * this.springConstant;
        let forceY = springVectorY * this.springConstant;
        //the spring force is added to the force

        const dVelX = this.mass2.vel.x - this.mass1.vel.x;
        const dVelY = this.mass2.vel.y - this.mass1.vel.y;

        //            if (dVel.Length > 1)
        forceX += dVelX * this.frictionConstant;
        forceY += dVelY * this.frictionConstant;
        //the friction force is added to the force
        //with this addition we obtain the net force of the spring

        this.mass1.applyForce(forceX, forceY); //force is applied to _mass1
        this.mass2.applyForce(-forceX, -forceY); //the opposite of force is applied to mass2
    }
}